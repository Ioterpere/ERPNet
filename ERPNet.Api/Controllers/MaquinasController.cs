using ERPNet.Api.Attributes;
using ERPNet.Api.Controllers.Common;
using ERPNet.Application.FileStorage;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Repositories;

namespace ERPNet.Api.Controllers;

[Recurso(RecursoCodigo.Maquinaria)]
public class MaquinasController(
    IFileStorageService fileStorage,
    IMaquinariaRepository repo,
    IUnitOfWork unitOfWork)
    : ArchivoBaseController<Maquinaria, CampoArchivoMaquinaria>(fileStorage, repo, unitOfWork)
{
}
