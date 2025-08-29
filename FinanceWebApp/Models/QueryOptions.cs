using System.Linq.Expressions;

namespace FinanceWebApp.Models;

public class QueryOptions<T> where T : class
{
    public Expression<Func<T, bool>>? Where { get; set; } = null;
    private string[] _includes = Array.Empty<string>();

    public string Includes
    {
        set => _includes = value.Replace(" ", "").Split(',');
    }
    
    public string[] GetIncludes() =>  _includes;
    public bool HasWhere() => Where != null;
}