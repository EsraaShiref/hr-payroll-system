using HRPayroll.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HRPayroll.Infrastructure.Persistence.ValueConverters;

public class EmployeeCodeConverter : ValueConverter<EmployeeCode, string>
{
    public EmployeeCodeConverter()
        : base(
            code => code.Value,
            value => EmployeeCode.Create(value))
    {
    }
}
