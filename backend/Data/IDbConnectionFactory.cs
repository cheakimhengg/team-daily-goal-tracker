using System.Data;

namespace backend.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
