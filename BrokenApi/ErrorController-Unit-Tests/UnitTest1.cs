using System;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BrokenApi.Data;
using BrokenApi.Models;
using BrokenApi.Controllers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ErrorController_Unit_Tests
{
    public class UnitTest1
    {
        [Fact]
        public async void DatabaseCanSaveErrors()
        {
            DbContextOptions<BrokenAPIContext> options =
                new DbContextOptionsBuilder<BrokenAPIContext>()
                .UseInMemoryDatabase("DbCanSave")
                .Options;

            using (BrokenAPIContext context = new BrokenAPIContext(options))
            {
                // Arrange
                Error newError = new Error
                {
                    ErrorCategoryID = 0,
                    DetailedName = "testError",
                    Description = "This is a test.",
                    Link = "test",
                    CodeExample = "test",
                    IsUserExample = false,
                    Votes = 0,
                    Rating = 0
                };

                // Act
                await context.Errors.AddAsync(newError);
                await context.SaveChangesAsync();

                var results = context.Errors.Where(e => e.DetailedName == "testError");

                // Assert
                Assert.Equal(1, results.Count());
            }
        }
        
        [Fact]
        public async void GetAllCanReturnAllErrorsInDatabase()
        {
            DbContextOptions<BrokenAPIContext> options =
                new DbContextOptionsBuilder<BrokenAPIContext>()
                .UseInMemoryDatabase("GetAllDb")
                .Options;

            using (BrokenAPIContext context = new BrokenAPIContext(options))
            {
                // Arrange
                ErrorController ec = new ErrorController(context);

                Error errorOne = new Error
                {
                    ErrorCategoryID = 0,
                    DetailedName = "errorOne",
                    Description = "This is a test.",
                    Link = "test",
                    CodeExample = "test",
                    IsUserExample = false,
                    Votes = 0,
                    Rating = 0
                };

                Error errorTwo = new Error
                {
                    ErrorCategoryID = 0,
                    DetailedName = "errorTwo",
                    Description = "This is a test.",
                    Link = "test",
                    CodeExample = "test",
                    IsUserExample = false,
                    Votes = 0,
                    Rating = 0
                };

                Error errorThree = new Error
                {
                    ErrorCategoryID = 0,
                    DetailedName = "errorThree",
                    Description = "This is a test.",
                    Link = "test",
                    CodeExample = "test",
                    IsUserExample = false,
                    Votes = 0,
                    Rating = 0
                };
                
                await context.Errors.AddAsync(errorOne);
                await context.Errors.AddAsync(errorTwo);
                await context.Errors.AddAsync(errorThree);
                await context.SaveChangesAsync();

                // Act
                var allErrors = ec.GetAll();

                // Assert
                Assert.Equal(3, allErrors.Result.Value.Count());
            }
        }
        
        [Fact]
        public async void GetAllCanReturnNullIfNoErrorsExistInDatabase()
        {
            DbContextOptions<BrokenAPIContext> options =
                new DbContextOptionsBuilder<BrokenAPIContext>()
                .UseInMemoryDatabase("GetAllDb")
                .Options;

            using (BrokenAPIContext context = new BrokenAPIContext(options))
            {
                // Arrange
                ErrorController ec = new ErrorController(context);
                
                // Act
                var allErrors = await ec.GetAll();

                // Assert
                Assert.Null(allErrors.Result);
            }
        }
        
        [Theory]
        [InlineData("firstTestError", 1)]
        [InlineData("secondTestError", 5)]
        [InlineData("thirdTestError", 10)]
        public async void GetTopCanReturnTheErrorWithTheMostUpVotes(string errName, int numVotes)
        {
            DbContextOptions<BrokenAPIContext> options =
                new DbContextOptionsBuilder<BrokenAPIContext>()
                .UseInMemoryDatabase(errName)
                .Options;

            using (BrokenAPIContext context = new BrokenAPIContext(options))
            {
                // Arrange
                ErrorController ec = new ErrorController(context);

                Error controlError = new Error
                {
                    ErrorCategoryID = 0,
                    DetailedName = "controlError",
                    Description = "This is a test.",
                    Link = "test",
                    CodeExample = "test",
                    IsUserExample = false,
                    Votes = 0,
                    Rating = 0
                };

                Error testError = new Error
                {
                    ErrorCategoryID = 0,
                    DetailedName = errName,
                    Description = "This is a test.",
                    Link = "test",
                    CodeExample = "test",
                    IsUserExample = false,
                    Votes = numVotes,
                    Rating = 0
                };

                await context.Errors.AddAsync(controlError);
                await context.Errors.AddAsync(testError);
                await context.SaveChangesAsync();

                // Act
                var topError = ec.GetTop();

                // Assert
                Assert.Equal(errName, topError.Result.Value.DetailedName);
            }
        }

        [Fact]
        public void GetTopCanReturnNullExceptionIfNoErrorsExistInDatabase()
        {
            DbContextOptions<BrokenAPIContext> options =
                new DbContextOptionsBuilder<BrokenAPIContext>()
                .UseInMemoryDatabase("GetTopdb")
                .Options;

            using (BrokenAPIContext context = new BrokenAPIContext(options))
            {
                // Arrange
                ErrorController ec = new ErrorController(context);

                // Act
                var response = ec.GetTop();

                // Assert
                Assert.Null(response.Exception);
            }
        }

        [Theory]
        [InlineData("firstTestError")]
        [InlineData("secondTestError")]
        [InlineData("thirdTestError")]
        public async void GetErrorCanReturnTheErrorFromSearch(string errName)
        {
            DbContextOptions<BrokenAPIContext> options =
                new DbContextOptionsBuilder<BrokenAPIContext>()
                .UseInMemoryDatabase(errName)
                .Options;

            using (BrokenAPIContext context = new BrokenAPIContext(options))
            {
                // Arrange
                ErrorController ec = new ErrorController(context);

                Error testError = new Error
                {
                    ErrorCategoryID = 0,
                    DetailedName = errName,
                    Description = "This is a test.",
                    Link = "test",
                    CodeExample = "test",
                    IsUserExample = false,
                    Votes = 0,
                    Rating = 0
                };

                await context.Errors.AddAsync(testError);
                await context.SaveChangesAsync();

                // Act
                var foundError = ec.GetError(errName);

                // Assert
                Assert.Equal(errName, foundError.Result.Value.DetailedName);
            }
        }

        [Fact]
        public async void GetErrorCanReturnBadRequestIfNoSearchNameProvided()
        {
            DbContextOptions<BrokenAPIContext> options =
                new DbContextOptionsBuilder<BrokenAPIContext>()
                .UseInMemoryDatabase("GetErrorDb")
                .Options;

            using (BrokenAPIContext context = new BrokenAPIContext(options))
            {
                // Arrange
                ErrorController ec = new ErrorController(context);

                Error testError = new Error
                {
                    ErrorCategoryID = 0,
                    DetailedName = "testError",
                    Description = "This is a test.",
                    Link = "test",
                    CodeExample = "test",
                    IsUserExample = false,
                    Votes = 0,
                    Rating = 0
                };

                await context.Errors.AddAsync(testError);
                await context.SaveChangesAsync();

                // Act
                var response = ec.GetError("");

                // Assert
                Assert.Equal("Microsoft.AspNetCore.Mvc.BadRequestResult", response.Result.Result.ToString());
            }
        }

        [Fact]
        public async void GetErrorCanReturnNotFoundIfErrorWithSearchNameDoesNotExistInDatabase()
        {
            DbContextOptions<BrokenAPIContext> options =
                new DbContextOptionsBuilder<BrokenAPIContext>()
                .UseInMemoryDatabase("GetErrorDb")
                .Options;

            using (BrokenAPIContext context = new BrokenAPIContext(options))
            {
                // Arrange
                ErrorController ec = new ErrorController(context);

                Error testError = new Error
                {
                    ErrorCategoryID = 0,
                    DetailedName = "testError",
                    Description = "This is a test.",
                    Link = "test",
                    CodeExample = "test",
                    IsUserExample = false,
                    Votes = 0,
                    Rating = 0
                };

                await context.Errors.AddAsync(testError);
                await context.SaveChangesAsync();

                // Act
                var response = ec.GetError("differentError");

                // Assert
                Assert.Equal("Microsoft.AspNetCore.Mvc.NotFoundResult", response.Result.Result.ToString());
            }
        }

        [Theory]
        [InlineData("firstPostTest")]
        [InlineData("secondPostTest")]
        [InlineData("thirdPostTest")]
        public async void PostErrorCanCreateNewErrors(string errorName)
        {
            DbContextOptions<BrokenAPIContext> options =
                new DbContextOptionsBuilder<BrokenAPIContext>()
                .UseInMemoryDatabase(errorName)
                .Options;

            using (BrokenAPIContext context = new BrokenAPIContext(options))
            {
                // Arrange
                ErrorController ec = new ErrorController(context);

                Error newError = new Error
                {
                    ErrorCategoryID = 0,
                    DetailedName = errorName,
                    Description = "This is a test.",
                    Link = "test",
                    CodeExample = "test",
                    IsUserExample = false,
                    Votes = 0,
                    Rating = 0
                };

                // Act
                await ec.PostError(newError);

                var results = context.Errors.Where(e => e.DetailedName == errorName);

                // Assert
                Assert.Equal(1, results.Count());
            }
        }

        [Fact]
        public async void PostErrorCanReturnConflictIfDetailedNameProvidedForErrorAlreadyExistsInDatabase()
        {
            DbContextOptions<BrokenAPIContext> options =
                new DbContextOptionsBuilder<BrokenAPIContext>()
                .UseInMemoryDatabase("postErrorSameName")
                .Options;

            using (BrokenAPIContext context = new BrokenAPIContext(options))
            {
                // Arrange
                ErrorController ec = new ErrorController(context);

                Error controlError = new Error
                {
                    ErrorCategoryID = 0,
                    DetailedName = "controlError",
                    Description = "This is a test.",
                    Link = "test",
                    CodeExample = "test",
                    IsUserExample = false,
                    Votes = 0,
                    Rating = 0
                };

                Error testError = new Error
                {
                    ErrorCategoryID = 0,
                    DetailedName = "controlError",
                    Description = "This is another test.",
                    Link = "test",
                    CodeExample = "test",
                    IsUserExample = false,
                    Votes = 0,
                    Rating = 0
                };

                await ec.PostError(controlError);

                // Act
                var response = await ec.PostError(testError);

                // Assert
                Assert.Equal("Microsoft.AspNetCore.Mvc.ConflictResult", response.ToString());
            }
        }

        /*
        [Fact]
        public async void PostErrorCanReturnArgumentNullExceptionIfNoDetailedNameIsProvidedForNewError()
        {
            DbContextOptions<BrokenAPIContext> options =
                new DbContextOptionsBuilder<BrokenAPIContext>()
                .UseInMemoryDatabase("postErrorNullName")
                .Options;

            using (BrokenAPIContext context = new BrokenAPIContext(options))
            {
                // Arrange
                ErrorController ec = new ErrorController(context);

                Error newError = new Error
                {
                    ErrorCategoryID = 0,
                    Description = "This is a test.",
                    Link = "test",
                    CodeExample = "test",
                    IsUserExample = false,
                    Votes = 0,
                    Rating = 0
                };

                // Act
                var response = await ec.PostError(newError);

                // Assert
                Assert.Null(response);
            }
        }
        */

        /*
        
        [Theory]
        [InlineData("firstAddVoteTest", 0)]
        [InlineData("secondAddVoteTest", 5)]
        [InlineData("thirdAddVoteTest", 10)]
        public async void AddVoteCanIncrementVoteCountForSpecifiedErrorAndReturnOk(string errorName, int startVoteCount)
        {
            DbContextOptions<BrokenAPIContext> options =
                new DbContextOptionsBuilder<BrokenAPIContext>()
                .UseInMemoryDatabase(errorName)
                .Options;

            using (BrokenAPIContext context = new BrokenAPIContext(options))
            {
                // Arrange
                ErrorController ec = new ErrorController(context);

                Error newError = new Error
                {
                    ErrorCategoryID = 0,
                    DetailedName = errorName,
                    Description = "This is a test.",
                    Link = "test",
                    CodeExample = "test",
                    IsUserExample = false,
                    Votes = startVoteCount,
                    Rating = 0
                };

                await ec.PostError(newError);

                int errorID = 0;
                switch (errorName)
                {
                    case "firstAddVoteTest":
                        errorID = 3;
                        break;
                    case "secondAddVoteTest":
                        errorID = 2;
                        break;
                    case "thirdAddVoteTest":
                        errorID = 1;
                        break;
                }

                // Act
                var response = await ec.AddVote(errorID);
                var error = ec.GetTop();

                // Assert
                Assert.Equal("Microsoft.AspNetCore.Mvc.OkResult", response.ToString());
                Assert.Equal(startVoteCount + 1, error.Result.Value.Votes);
            }
        }

        */

        [Fact]
        public async void AddVoteCanReturnNotFoundIfErrorNotFoundForGivenIDInDatabase()
        {
            DbContextOptions<BrokenAPIContext> options =
                new DbContextOptionsBuilder<BrokenAPIContext>()
                .UseInMemoryDatabase("addVoteIncorrectID")
                .Options;

            using (BrokenAPIContext context = new BrokenAPIContext(options))
            {
                // Arrange
                ErrorController ec = new ErrorController(context);

                Error newError = new Error
                {
                    ErrorCategoryID = 0,
                    DetailedName = "Test",
                    Description = "This is a testError.",
                    Link = "test",
                    CodeExample = "test",
                    IsUserExample = false,
                    Votes = 0,
                    Rating = 0
                };

                await ec.PostError(newError);

                // Act
                var response = await ec.AddVote(100);

                // Assert
                Assert.Equal("Microsoft.AspNetCore.Mvc.NotFoundResult", response.ToString());
            }
        }

        [Theory]
        [InlineData("firstDeleteErrorTest")]
        [InlineData("secondDeleteErrorTest")]
        [InlineData("thirdDeleteErrorTest")]
        public async void DeleteErrorCanRemoveAnErrorFromTheDatabaseAndReturnOk(string errorName)
        {
            DbContextOptions<BrokenAPIContext> options =
                new DbContextOptionsBuilder<BrokenAPIContext>()
                .UseInMemoryDatabase(errorName)
                .Options;

            using (BrokenAPIContext context = new BrokenAPIContext(options))
            {
                // Arrange
                ErrorController ec = new ErrorController(context);

                Error newError = new Error
                {
                    ErrorCategoryID = 0,
                    DetailedName = errorName,
                    Description = "This is a test.",
                    Link = "test",
                    CodeExample = "test",
                    IsUserExample = false,
                    Votes = 0,
                    Rating = 0
                };

                await ec.PostError(newError);

                int errorID = 0;
                switch (errorName)
                {
                    case "firstDeleteErrorTest":
                        errorID = 1;
                        break;
                    case "secondDeleteErrorTest":
                        errorID = 2;
                        break;
                    case "thirdDeleteErrorTest":
                        errorID = 3;
                        break;
                }

                // Act
                var response = await ec.DeleteError(errorID);
                var results = context.Errors.Where(e => e.DetailedName == errorName);
                
                // Assert
                Assert.Equal("Microsoft.AspNetCore.Mvc.OkResult", response.ToString());
                Assert.Equal(0, results.Count());
            }
        }

        [Fact]
        public async void DeleteErrorCanReturnNotFoundIfErrorNotFoundForGivenIDInDatabase()
        {
            DbContextOptions<BrokenAPIContext> options =
                new DbContextOptionsBuilder<BrokenAPIContext>()
                .UseInMemoryDatabase("deleteErrorIncorrectID")
                .Options;

            using (BrokenAPIContext context = new BrokenAPIContext(options))
            {
                // Arrange
                ErrorController ec = new ErrorController(context);

                Error newError = new Error
                {
                    ErrorCategoryID = 0,
                    DetailedName = "Test",
                    Description = "This is a testError.",
                    Link = "test",
                    CodeExample = "test",
                    IsUserExample = false,
                    Votes = 0,
                    Rating = 0
                };

                await ec.PostError(newError);

                // Act
                var response = await ec.DeleteError(100);

                // Assert
                Assert.Equal("Microsoft.AspNetCore.Mvc.NotFoundResult", response.ToString());
            }
        }
    }
}
