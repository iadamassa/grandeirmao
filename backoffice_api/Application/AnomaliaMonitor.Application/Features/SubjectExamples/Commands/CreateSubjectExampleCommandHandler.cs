using MediatR;
using AutoMapper;
using AnomaliaMonitor.Domain.Entities;
using AnomaliaMonitor.Domain.Interfaces;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Application.Features.SubjectExamples.Commands;

public class CreateSubjectExampleCommandHandler : IRequestHandler<CreateSubjectExampleCommand, SubjectExampleDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateSubjectExampleCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SubjectExampleDto> Handle(CreateSubjectExampleCommand request, CancellationToken cancellationToken)
    {
        var example = new SubjectExample
        {
            Title = request.Name,
            Description = request.Description,
            SubjectToResearchId = request.SubjectToResearchId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.SubjectExamples.AddAsync(example);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<SubjectExampleDto>(example);
    }
}