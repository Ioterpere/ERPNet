using ERPNet.Application.Auth.Interfaces;
using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.DTOs.Mappings;
using ERPNet.Application.Common.Enums;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Filters;
using ERPNet.Domain.Repositories;

namespace ERPNet.Application.Common;

public class ArticuloService(
    IArticuloRepository articuloRepository,
    IFamiliaArticuloRepository familiaRepository,
    ICatalogoArticulosRepository catalogoRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserProvider currentUser) : IArticuloService
{
    public async Task<Result<ListaPaginada<ArticuloResponse>>> GetAllAsync(PaginacionFilter filtro)
    {
        var empresaId = currentUser.Current!.EmpresaId ?? 0;
        var (articulos, total) = await articuloRepository.GetPaginatedAsync(filtro, empresaId);
        var response = articulos.Select(a => a.ToResponse()).ToList();
        return Result<ListaPaginada<ArticuloResponse>>.Success(
            ListaPaginada<ArticuloResponse>.Crear(response, total));
    }

    public async Task<Result<ArticuloResponse>> GetByIdAsync(int id)
    {
        var articulo = await articuloRepository.GetByIdAsync(id);

        if (articulo is null)
            return Result<ArticuloResponse>.Failure("Artículo no encontrado.", ErrorType.NotFound);

        if (!TieneAcceso(articulo))
            return Result<ArticuloResponse>.Failure("No tiene acceso a este artículo.", ErrorType.Forbidden);

        return Result<ArticuloResponse>.Success(articulo.ToResponse());
    }

    public async Task<Result<ArticuloResponse>> CreateAsync(CreateArticuloRequest request)
    {
        var empresaId = currentUser.Current!.EmpresaId ?? 0;

        if (await articuloRepository.ExisteCodigoAsync(request.Codigo, empresaId))
            return Result<ArticuloResponse>.Failure("Ya existe un artículo con ese código.", ErrorType.Conflict);

        var articulo = request.ToEntity(empresaId);

        await articuloRepository.CreateAsync(articulo);
        await unitOfWork.SaveChangesAsync();

        return Result<ArticuloResponse>.Success(articulo.ToResponse());
    }

    public async Task<Result> UpdateAsync(int id, UpdateArticuloRequest request)
    {
        var articulo = await articuloRepository.GetByIdAsync(id);

        if (articulo is null)
            return Result.Failure("Artículo no encontrado.", ErrorType.NotFound);

        if (!TieneAcceso(articulo))
            return Result.Failure("No tiene acceso a este artículo.", ErrorType.Forbidden);

        if (request.Codigo is not null && request.Codigo != articulo.Codigo)
        {
            if (await articuloRepository.ExisteCodigoAsync(request.Codigo, articulo.EmpresaId, id))
                return Result.Failure("Ya existe un artículo con ese código.", ErrorType.Conflict);
        }

        request.ApplyTo(articulo);
        await unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var articulo = await articuloRepository.GetByIdAsync(id);

        if (articulo is null)
            return Result.Failure("Artículo no encontrado.", ErrorType.NotFound);

        if (!TieneAcceso(articulo))
            return Result.Failure("No tiene acceso a este artículo.", ErrorType.Forbidden);

        articuloRepository.Delete(articulo);
        await unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<List<ArticuloLogResponse>>> GetLogsAsync(int articuloId)
    {
        var articulo = await articuloRepository.GetByIdAsync(articuloId);

        if (articulo is null)
            return Result<List<ArticuloLogResponse>>.Failure("Artículo no encontrado.", ErrorType.NotFound);

        if (!TieneAcceso(articulo))
            return Result<List<ArticuloLogResponse>>.Failure("No tiene acceso a este artículo.", ErrorType.Forbidden);

        var logs = await articuloRepository.GetLogsAsync(articuloId);
        return Result<List<ArticuloLogResponse>>.Success(logs.Select(l => l.ToResponse()).ToList());
    }

    public async Task<Result<ArticuloLogResponse>> AddLogAsync(int articuloId, CreateArticuloLogRequest request)
    {
        var articulo = await articuloRepository.GetByIdAsync(articuloId);

        if (articulo is null)
            return Result<ArticuloLogResponse>.Failure("Artículo no encontrado.", ErrorType.NotFound);

        if (!TieneAcceso(articulo))
            return Result<ArticuloLogResponse>.Failure("No tiene acceso a este artículo.", ErrorType.Forbidden);

        var log = new ArticuloLog
        {
            ArticuloId    = articuloId,
            UsuarioId     = currentUser.Current!.Id,
            Fecha         = request.Fecha,
            Nota          = request.Nota,
            StockAnterior = request.StockAnterior,
            StockNuevo    = request.StockNuevo,
            CreatedAt     = DateTime.UtcNow,
        };

        articulo.Logs.Add(log);
        await unitOfWork.SaveChangesAsync();

        return Result<ArticuloLogResponse>.Success(log.ToResponse());
    }

    public async Task<Result<List<FamiliaArticuloResponse>>> GetFamiliasAsync()
    {
        var empresaId = currentUser.Current!.EmpresaId ?? 0;
        var familias = await familiaRepository.GetAllByEmpresaAsync(empresaId);
        return Result<List<FamiliaArticuloResponse>>.Success(familias.Select(f => f.ToResponse()).ToList());
    }

    public async Task<Result<List<TipoIvaResponse>>> GetTiposIvaAsync()
    {
        var tipos = await catalogoRepository.GetTiposIvaAsync();
        return Result<List<TipoIvaResponse>>.Success(tipos.Select(t => t.ToResponse()).ToList());
    }

    public async Task<Result<List<FormatoArticuloResponse>>> GetFormatosAsync()
    {
        var formatos = await catalogoRepository.GetFormatosAsync();
        return Result<List<FormatoArticuloResponse>>.Success(formatos.Select(f => f.ToResponse()).ToList());
    }

    private bool TieneAcceso(Articulo articulo)
    {
        var empresaId = currentUser.Current!.EmpresaId;
        return empresaId is null || articulo.EmpresaId == empresaId;
    }
}
