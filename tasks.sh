#!/usr/bin/env bash
set -e

PROJECT="ERPNet.Infrastructure"
STARTUP="ERPNet.Api"
MIGRATIONS="Database/Migrations"

# Lee la connection string completa de appsettings.json
get_connection() {
  grep '"DefaultConnection"' "$STARTUP/appsettings.json" | sed 's/.*": *"//;s/".*//'
}

# Muestra Server -> Database y pide confirmacion
confirm_db() {
  local conn server db
  conn=$(get_connection)
  server=$(echo "$conn" | grep -oP 'Server=\K[^;]+')
  db=$(echo "$conn" | grep -oP 'Database=\K[^;]+')
  echo "  Destino: $server -> $db"
  read -rp "  Continuar? (s/N): " resp
  [[ "$resp" =~ ^[sS]$ ]]
}

require_docker() {
  if ! command -v docker &>/dev/null; then
    echo "  Docker no encontrado. Instalar Docker Desktop:" >&2
    echo "  https://docs.docker.com/desktop/setup/install/windows-install/" >&2
    return 1
  fi
}

run_task() {
  case "$1" in
    1|build)
      dotnet build ERPNet.slnx
      ;;
    2|test)
      dotnet test ERPNet.Testing --filter "FullyQualifiedName!~DbSeeder"
      ;;
    3|migration)
      local name="$2"
      if [ -z "$name" ]; then
        read -rp "  Nombre de la migracion: " name
        if [ -z "$name" ]; then
          echo "  Nombre requerido." >&2
          return 1
        fi
      fi
      dotnet ef migrations add "$name" --project "$PROJECT" --startup-project "$STARTUP" --output-dir "$MIGRATIONS"
      ;;
    4|migrate)
      if ! confirm_db; then
        echo "  Cancelado."
        return 0
      fi
      dotnet ef database update --project "$PROJECT" --startup-project "$STARTUP"
      ;;
    5|seed)
      dotnet test ERPNet.Testing --filter "FullyQualifiedName~DbSeeder"
      ;;
    6|drop)
      if ! confirm_db; then
        echo "  Cancelado."
        return 0
      fi
      dotnet ef database drop --force --project "$PROJECT" --startup-project "$STARTUP"
      ;;
    7|dockup)
      require_docker || return 1
      docker compose up -d
      ;;
    8|dockdown)
      require_docker || return 1
      docker compose down
      ;;
    9|clearminio)
      require_docker || return 1
      local bucket
      bucket=$(grep '"BucketName"' "$STARTUP/appsettings.json" | sed 's/.*": *"//;s/".*//')
      echo "  Vaciando bucket: $bucket"
      docker compose exec minio sh -c "mc alias set local http://localhost:9000 erpnet ERPNet_Dev123! --api S3v4 >/dev/null 2>&1 && mc rm --recursive --force local/$bucket" 2>/dev/null || true
      echo "  Bucket vaciado."
      ;;
    0|exit)
      exit 0
      ;;
    *)
      echo "  Opcion no valida." >&2
      return 1
      ;;
  esac
}

# Modo directo: ./tasks.sh build
if [ -n "$1" ]; then
  run_task "$@"
  exit $?
fi

# Modo interactivo
while true; do
  echo ""
  echo "  ERPNet - Tareas"
  echo "  ---------------"
  echo "  1) build        Compilar solucion"
  echo "  2) test         Ejecutar tests"
  echo "  3) migration    Crear migracion"
  echo "  4) migrate      Aplicar migraciones"
  echo "  5) seed         Ejecutar seeder"
  echo "  6) drop         Borrar DB"
  echo "  7) dockup       Levantar contenedores"
  echo "  8) dockdown     Parar contenedores"
  echo "  9) clearminio   Vaciar bucket MinIO"
  echo "  0) salir"
  echo ""
  read -rp "  > " choice
  echo ""
  run_task "$choice" || true
done
