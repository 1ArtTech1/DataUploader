using System.Data;

namespace DataUploader.Services.Interfaces;

public interface ICabService
{
    Task<DataTable> CreateCabsAsync();
}