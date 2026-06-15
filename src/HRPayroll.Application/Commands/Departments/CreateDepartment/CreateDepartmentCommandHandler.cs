using ErrorOr;
using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;
using MediatR;

namespace HRPayroll.Application.Commands.Departments.CreateDepartment;

public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, ErrorOr<Guid>>
{
    private readonly IRepository<Department> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDepartmentCommandHandler(IRepository<Department> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Guid>> Handle(CreateDepartmentCommand command, CancellationToken ct)
    {
        var department = new Department(command.Name, command.Code, command.Description, command.ParentDepartmentId);
        _repository.Add(department);
        await _unitOfWork.SaveChangesAsync(ct);
        return department.Id;
    }
}
