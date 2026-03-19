using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OctocatSupply.Api.Controllers;
using OctocatSupply.Api.Models;
using OctocatSupply.Api.Repositories;

namespace OctocatSupply.Api.UnitTests.Controllers;

/// <summary>
/// Unit tests for the <see cref="BranchesController"/> class.
/// </summary>
[TestClass]
public class BranchesControllerTests
{
    private Mock<IBranchRepository> _mockRepository = null!;
    private BranchesController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<IBranchRepository>();
        _controller = new BranchesController(_mockRepository.Object);
    }

    /// <summary>
    /// Tests that creating a new branch returns 201 Created with the branch data.
    /// </summary>
    [TestMethod]
    public async Task Create_WithValidBranch_Returns201CreatedWithBranch()
    {
        // Arrange
        var newBranch = new Branch
        {
            HeadquartersId = 1,
            Name = "Eastside Branch",
            Description = "Eastern district branch",
            Address = "321 East St",
            ContactPerson = "Emma Davis",
            Email = "edavis@octo.com",
            Phone = "555-0203"
        };

        var createdBranch = new Branch
        {
            BranchId = 1,
            HeadquartersId = 1,
            Name = "Eastside Branch",
            Description = "Eastern district branch",
            Address = "321 East St",
            ContactPerson = "Emma Davis",
            Email = "edavis@octo.com",
            Phone = "555-0203"
        };

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Branch>()))
            .ReturnsAsync(createdBranch);

        // Act
        var result = await _controller.Create(newBranch);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
        var createdResult = (CreatedAtActionResult)result.Result!;
        Assert.AreEqual(201, createdResult.StatusCode);
        var returnedBranch = (Branch)createdResult.Value!;
        Assert.AreEqual("Eastside Branch", returnedBranch.Name);
        Assert.AreEqual(1, returnedBranch.HeadquartersId);
        Assert.AreEqual("321 East St", returnedBranch.Address);
        Assert.AreEqual("Emma Davis", returnedBranch.ContactPerson);
        Assert.AreEqual("edavis@octo.com", returnedBranch.Email);
        Assert.AreEqual("555-0203", returnedBranch.Phone);
        Assert.AreEqual(1, returnedBranch.BranchId);
    }

    /// <summary>
    /// Tests that getting all branches returns 200 OK with a list of branches.
    /// </summary>
    [TestMethod]
    public async Task GetAll_ReturnsOkWithListOfBranches()
    {
        // Arrange
        var branches = new List<Branch>
        {
            new Branch { BranchId = 1, HeadquartersId = 1, Name = "Branch One" },
            new Branch { BranchId = 2, HeadquartersId = 1, Name = "Branch Two" }
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(branches);

        // Act
        var result = await _controller.GetAll();

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = (OkObjectResult)result.Result!;
        var returnedBranches = (IEnumerable<Branch>)okResult.Value!;
        Assert.AreEqual(2, returnedBranches.Count());
    }

    /// <summary>
    /// Tests that getting all branches returns 200 OK with an empty list when no branches exist.
    /// </summary>
    [TestMethod]
    public async Task GetAll_WhenNoBranches_ReturnsOkWithEmptyList()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Branch>());

        // Act
        var result = await _controller.GetAll();

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = (OkObjectResult)result.Result!;
        var returnedBranches = (IEnumerable<Branch>)okResult.Value!;
        Assert.AreEqual(0, returnedBranches.Count());
    }

    /// <summary>
    /// Tests that getting a branch by ID returns 200 OK with the branch when it exists.
    /// </summary>
    [TestMethod]
    public async Task GetById_WithExistingId_ReturnsOkWithBranch()
    {
        // Arrange
        var branch = new Branch
        {
            BranchId = 1,
            HeadquartersId = 1,
            Name = "Test Branch",
            Description = "Test branch",
            Address = "123 Test St",
            ContactPerson = "Test Person",
            Email = "test@test.com",
            Phone = "555-0000"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(branch);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = (OkObjectResult)result.Result!;
        var returnedBranch = (Branch)okResult.Value!;
        Assert.AreEqual(1, returnedBranch.BranchId);
        Assert.AreEqual("Test Branch", returnedBranch.Name);
    }

    /// <summary>
    /// Tests that updating a branch returns 200 OK with the updated branch data.
    /// </summary>
    [TestMethod]
    public async Task Update_WithExistingId_ReturnsOkWithUpdatedBranch()
    {
        // Arrange
        var updatedBranch = new Branch
        {
            BranchId = 1,
            HeadquartersId = 1,
            Name = "Updated Branch Name",
            Description = "Original description",
            Address = "123 Original St",
            ContactPerson = "Original Person",
            Email = "original@test.com",
            Phone = "555-0001"
        };

        _mockRepository.Setup(r => r.UpdateAsync(1, It.IsAny<Branch>()))
            .ReturnsAsync(updatedBranch);

        // Act
        var result = await _controller.Update(1, updatedBranch);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = (OkObjectResult)result.Result!;
        var returnedBranch = (Branch)okResult.Value!;
        Assert.AreEqual("Updated Branch Name", returnedBranch.Name);
    }

    /// <summary>
    /// Tests that deleting an existing branch returns 204 No Content.
    /// </summary>
    [TestMethod]
    public async Task Delete_WithExistingId_ReturnsNoContent()
    {
        // Arrange
        _mockRepository.Setup(r => r.DeleteAsync(1))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NoContentResult));
        var noContentResult = (NoContentResult)result;
        Assert.AreEqual(204, noContentResult.StatusCode);
    }

    /// <summary>
    /// Tests that getting a non-existing branch returns 404 Not Found.
    /// </summary>
    [TestMethod]
    public async Task GetById_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Branch?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
    }

    /// <summary>
    /// Tests that updating a non-existing branch returns 404 Not Found.
    /// </summary>
    [TestMethod]
    public async Task Update_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var branch = new Branch
        {
            HeadquartersId = 1,
            Name = "Ghost Branch"
        };

        _mockRepository.Setup(r => r.UpdateAsync(999, It.IsAny<Branch>()))
            .ThrowsAsync(new KeyNotFoundException());

        // Act
        var result = await _controller.Update(999, branch);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
    }

    /// <summary>
    /// Tests that deleting a non-existing branch returns 404 Not Found.
    /// </summary>
    [TestMethod]
    public async Task Delete_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        _mockRepository.Setup(r => r.DeleteAsync(999))
            .ThrowsAsync(new KeyNotFoundException());

        // Act
        var result = await _controller.Delete(999);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }
}
