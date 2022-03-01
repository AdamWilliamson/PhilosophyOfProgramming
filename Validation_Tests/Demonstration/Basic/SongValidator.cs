using Validations;

namespace Validations_Tests.Demonstration.Basic;

public class SongValidator : AbstractValidator<Song>
{
    public SongValidator()
    {
        Describe(x => x.TrackNumber).IsEqualTo(1);
        Describe(x => x.TrackName)
            .Vitally.IsNotEmpty()
            .IsEqualTo("A Song Title").When("TrackNumber is 1", (context, instance) => instance.TrackNumber == 1);
        Describe(x => x.Duration).IsEqualTo(2.3).WithErrorMessage("TrackNumber 1 must have a Duration of 2.3 minutes.");
        Describe(x => x.Description)
            .Vitally.IsNotNull()
            .HasLengthBetween(20, 400)
            .Is(AValidDescription, 
                "Should have a valid description", 
                "The description is invalid");
    }

    private bool AValidDescription(string description) { return description.Contains("song"); }
}