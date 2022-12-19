//using FluentAssertions;
//using FluentAssertions.Execution;
//using System.Collections.Generic;
//using System.Linq;
//using Xunit;

//namespace Validations_Tests.TheNewVersion;

//public struct Artist
//{
//    public string FirstName { get; set; }
//    public string LastName { get; set; }
//}

//public class Song
//{
//    public int TrackNumber { get; set; }
//    public string TrackName { get; set; }
//    public double Duration { get; set; }
//    public string? Description { get; set; }

//    public Song(int trackNumber, string trackName, double duration, string? description = null)
//    {
//        TrackNumber = trackNumber;
//        TrackName = trackName;
//        Duration = duration;
//        Description = description;
//    }
//}

//public class Album
//{
//    public List<Song> SongList { get; set; } = new()
//    {
//        new Song(1, "The First Song", 1.2)
//    };

//    public Artist Artist { get; set; } = new()
//    {
//        FirstName = "John",
//        LastName = "Williamson"
//    };

//    public string Name { get; set; }

//    public Album(string name)
//    {
//        Name = name;
//    }
//}

//public class SongValidator : AbstractValidator<Song>
//{
//    public SongValidator()
//    {
//        Describe(x => x.TrackNumber)
//            .IsEqualTo(1);
//        //Describe(x => x.TrackName)
//        //    .Vitally.IsNotNullOrEmpty()
//        //    .IsEqualTo("A Song Title").When("TrackNumber is 1", (context, instance) => instance.TrackNumber == 1);
//        //Describe(x => x.Duration)
//        //    .IsEqualTo(2.3).WithErrorMessage("TrackNumber 1 must have a Duration of 2.3 minutes.");
//        //Describe(x => x.Description)
//        //    .Vitally.IsNotNull()
//        //    .HasLengthBetween(20, 400)
//        //    .Is(AValidDescription,
//        //        "Should have a valid description",
//        //        "The description is invalid");
//    }

//    private bool AValidDescription(string description) { return description.Contains("song"); }
//}

//public interface IRepo { int GetValue(); }
//public class Repo : IRepo { public int GetValue() { return 1; } }
//public class AlbumValidator : AbstractValidator<Album>
//{
//    public AlbumValidator(IRepo repo)
//    {
//        Describe(x => x.Artist.FirstName)
//            .IsEqualTo("DefinitelyNotTheCorrectFirstName");
//        //    .Is(
//        //        "Should have a valid description",
//        //        "The description is invalid",
//        //        AValidDescription);

//        When(
//            "When something",
//            (validatedObject) => true,
//            () =>
//            {
//                Describe(x => x.Artist.FirstName).Vitally.IsEqualTo("Frenchy");
//            });

//        //ScopeWhen(
//        //    "When something",
//        //    (validatedObject) => true, 
//        //    (validatedObject) => repo.GetValue(), 
//        //    () =>
//        //    {
//        //        Describe(x => x.Artist.LastName).IsEqualTo("Frenchy");
//        //    });

//        //Include(new SecondaryAlbumValidator());

//        //Describe(x => x.SongList)
//        //    .Vitally.IsNotNull()
//        //    .ForEach(
//        //    item =>
//        //    {
//        //        item.Describe(x => x.TrackNumber).IsEqualTo(1);
//        //        item.Describe(x => x.TrackName)
//        //            .Vitally.IsNotEmpty()
//        //            .IsEqualTo("A Song Title").When("TrackNumber is 1", (context, instance) => instance.TrackNumber == 1);
//        //    });
//    }

//    private bool AValidDescription(string description) { return description.Contains("song"); }
//}

//public class SecondaryAlbumValidator : AbstractSubValidator<Album>
//{
//    public SecondaryAlbumValidator()
//    {
//        //Describe(x => x.Name).Vitally.NotNullOrEmpty();
//    }
//}

//public class OnlyTestsThatMatter
//{
//    [Fact]
//    public void BasicValidator_Validate_Validates()
//    {
//        // Arrange
//        var runner = new ValidationRunner<Album>(new List<IValidator<Album>>() { new AlbumValidator(new Repo()) });

//        // Act
//        var result = runner.Validate(new Album("Mighty Morphin Power Rangers - SoundTrak"));

//        // Assert
//        using (new AssertionScope())
//        {
//            result.Should().NotBeNull();
//            result.Errors.Should().NotBeNull();
//            result.Errors.Where(f => f.FieldName == "Artist.FirstName").Should().HaveCount(2);

//        }
//    }

//    [Fact]
//    public void BasicValidator_Describe_Describes()
//    {
//        // Arrange
//        var runner = new ValidationRunner<Album>(new List<IValidator<Album>>() { new AlbumValidator(new Repo()) });

//        // Act
//        var result = runner.Describe(new Album("Mighty Morphin Power Rangers - SoundTrak"));

//        // Assert
//        using (new AssertionScope())
//        {
//            result.Should().NotBeNull();
//            result.Descriptions.Should().NotBeNull();
//            result.Descriptions.Where(f => f.FieldName == "Artist.FirstName").Should().HaveCount(1);
//            result.Descriptions.Single(f => f.FieldName == "Artist.FirstName").Descriptions.Should().HaveCount(1);
//            result.Descriptions.Single(f => f.FieldName == "Artist.FirstName").Descriptions.Select(s => s.DescriptionMessage).Should().HaveCount(1);
//            result.Descriptions.Single(f => f.FieldName == "Artist.FirstName").Descriptions.Select(s => s.ErrorMessage).Should().HaveCount(1);
//            result.Descriptions.Single(f => f.FieldName == "Artist.FirstName").Descriptions.Select(s => s.ErrorMessage).Single().Should().Be("Is Not Equal To DefinitelyNotTheCorrectFirstName");
//            result.Descriptions.Single(f => f.FieldName == "Artist.FirstName").Descriptions.Select(s => s.DescriptionMessage).Single().Should().Be("Should Not Be Equal To DefinitelyNotTheCorrectFirstName");
//        }
//    }
//}
