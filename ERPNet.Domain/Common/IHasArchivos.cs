namespace ERPNet.Domain.Common;

public interface IHasArchivos<TEnum> where TEnum : struct, Enum
{
    Guid? GetArchivoId(TEnum campo);
    void SetArchivoId(TEnum campo, Guid? id);
}
