using FluentValidation;
using InsureZenv2.src.DTOs;

namespace InsureZenv2.src.Validators;

public class ClaimIngestDtoValidator : AbstractValidator<ClaimIngestDto>
{
    public ClaimIngestDtoValidator()
    {
        RuleFor(c => c.PatientName)
            .NotEmpty().WithMessage("Patient name is required")
            .MaximumLength(255).WithMessage("Patient name cannot exceed 255 characters");

        RuleFor(c => c.PatientId)
            .NotEmpty().WithMessage("Patient ID is required")
            .MaximumLength(100).WithMessage("Patient ID cannot exceed 100 characters");

        RuleFor(c => c.ServiceDate)
            .NotEmpty().WithMessage("Service date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Service date cannot be in the future");

        RuleFor(c => c.ClaimAmount)
            .NotEmpty().WithMessage("Claim amount is required")
            .GreaterThan(0).WithMessage("Claim amount must be greater than 0");

        RuleFor(c => c.ServiceDescription)
            .NotEmpty().WithMessage("Service description is required");

        RuleFor(c => c.ProviderName)
            .NotEmpty().WithMessage("Provider name is required")
            .MaximumLength(255).WithMessage("Provider name cannot exceed 255 characters");

        RuleFor(c => c.ProviderCode)
            .NotEmpty().WithMessage("Provider code is required")
            .MaximumLength(100).WithMessage("Provider code cannot exceed 100 characters");
    }
}

public class MakerReviewSubmitDtoValidator : AbstractValidator<MakerReviewSubmitDto>
{
    public MakerReviewSubmitDtoValidator()
    {
        RuleFor(r => r.Feedback)
            .NotEmpty().WithMessage("Feedback is required");

        RuleFor(r => r.Recommendation)
            .NotEmpty().WithMessage("Recommendation is required")
            .Must(r => r == "Approve" || r == "Reject")
            .WithMessage("Recommendation must be either 'Approve' or 'Reject'");
    }
}

public class CheckerReviewSubmitDtoValidator : AbstractValidator<CheckerReviewSubmitDto>
{
    public CheckerReviewSubmitDtoValidator()
    {
        RuleFor(r => r.Feedback)
            .NotEmpty().WithMessage("Feedback is required");

        RuleFor(r => r.Decision)
            .NotEmpty().WithMessage("Decision is required")
            .Must(r => r == "Approved" || r == "Rejected")
            .WithMessage("Decision must be either 'Approved' or 'Rejected'");
    }
}
