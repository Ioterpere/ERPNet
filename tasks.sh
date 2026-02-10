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
    6|dropseed)
      if ! confirm_db; then
        echo "  Cancelado."
        return 0
      fi
      dotnet ef database drop --force --project "$PROJECT" --startup-project "$STARTUP"
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
  echo "  6) dropseed     Borrar DB"
  echo "  0) salir"
  echo ""
  read -rp "  > " choice
  echo ""
  run_task "$choice" || true
done
