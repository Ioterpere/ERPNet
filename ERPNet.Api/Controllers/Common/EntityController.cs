// using ERPNet.Application;
// using ERPNet.Application.Enums;
// using ERPNet.Domain.Common;
// using ERPNet.Domain.Repositories;
// using Microsoft.AspNetCore.Mvc;

// namespace ERPNet.Api.Controllers.Common;

// public abstract class EntityController<T, TResponse, TCreateRequest>(
//     IRepository<T> repo,
//     IUnitOfWork unitOfWork) : BaseController where T : BaseEntity
// {
//     protected readonly IRepository<T> _repo = repo;
//     protected readonly IUnitOfWork _unitOfWork = unitOfWork;
//     protected abstract TResponse MapToResponse(T entity);
//     protected abstract T MapToEntity(TCreateRequest request);
//     // protected abstract void MapUpdateToEntity(T entity, TUpdateRequest request);

//     [HttpGet]
//     public virtual async Task<IActionResult> GetAll()
//     {
//         var entities = await _repo.GetAllAsync();
//         var response = entities.Select(MapToResponse).ToList();
//         return FromResult(Result<List<TResponse>>.Success(response));
//     }

//     [HttpGet("{id}")]
//     public virtual async Task<IActionResult> GetById(int id)
//     {
//         var entity = await _repo.GetByIdAsync(id);
//         if (entity is null) return NotFound();

//         return FromResult(Result<TResponse>.Success(MapToResponse(entity)));
//     }

//     [HttpPost]
//     public virtual async Task<IActionResult> Create([FromBody] TCreateRequest request)
//     {
//         var entity = MapToEntity(request);

//         await _repo.CreateAsync(entity);
//         await _unitOfWork.SaveChangesAsync();

//         await OnAfterUpdateAsync(entity);

//         return CreatedFromResult(
//             Result<TResponse>.Success(MapToResponse(entity)),
//             nameof(GetById),
//             new { id = entity.Id });
//     }

//     [HttpPut("{id}")]
//     public virtual async Task<IActionResult> Update(int id, [FromBody] T request)
//     {
//         var entity = await _repo.GetByIdAsync(id);

//         if (entity is null)
//             return FromResult(Result.Failure($"No se pudo encontrar {typeof(T).Name}.", ErrorType.NotFound));

//         // MapUpdateToEntity(entity, request);

//         _repo.Update(entity);
//         await _unitOfWork.SaveChangesAsync();

//         await OnAfterUpdateAsync(entity);

//         return FromResult(Result.Success());
//     }

//     [HttpDelete("{id}")]
//     public virtual async Task<IActionResult> Delete(int id)
//     {
//         var entity = await _repo.GetByIdAsync(id);

//         if (entity is null)
//             return FromResult(Result.Failure($"No se pudo encontrar {typeof(T).Name}.", ErrorType.NotFound));

//         _repo.Delete(entity);
//         await _unitOfWork.SaveChangesAsync();

//         await OnAfterDeleteAsync(entity);

//         return FromResult(Result.Success());
//     }

//     protected virtual Task OnAfterUpdateAsync(T entity) => Task.CompletedTask;
//     protected virtual Task OnAfterDeleteAsync(T entity) => Task.CompletedTask;
// }
