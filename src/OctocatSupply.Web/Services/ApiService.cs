namespace OctocatSupply.Web.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<T>> GetAllAsync<T>(string endpoint)
    {
        var response = await _httpClient.GetFromJsonAsync<List<T>>(endpoint);
        return response ?? new List<T>();
    }

    public async Task<T?> GetByIdAsync<T>(string endpoint, int id)
    {
        return await _httpClient.GetFromJsonAsync<T>($"{endpoint}/{id}");
    }

    public async Task<T?> CreateAsync<T>(string endpoint, T item)
    {
        var response = await _httpClient.PostAsJsonAsync(endpoint, item);
        return await response.Content.ReadFromJsonAsync<T>();
    }

    public async Task<T?> UpdateAsync<T>(string endpoint, int id, T item)
    {
        var response = await _httpClient.PutAsJsonAsync($"{endpoint}/{id}", item);
        return await response.Content.ReadFromJsonAsync<T>();
    }

    public async Task DeleteAsync(string endpoint, int id)
    {
        await _httpClient.DeleteAsync($"{endpoint}/{id}");
    }
}
