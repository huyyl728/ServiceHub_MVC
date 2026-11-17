
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHubMVC.Controllers;
using Xunit;

namespace ServiceHubMVC.BookingManagement.Tests;

public class BookingsControllerSecurityTests
{
    [Theory]
    [InlineData("Cancel", "Customer")]
    [InlineData("Confirm", "Owner")]
    [InlineData("Reject", "Owner")]
    [InlineData("Complete", "Owner")]
    public void PostActions_CoPhanQuyenVaChongCSRF(
        string actionName,
        string expectedRole)
    {
        var controllerType = typeof(BookingsController);

        Assert.NotNull(
            controllerType.GetCustomAttribute<AuthorizeAttribute>());

        var method = controllerType.GetMethod(
            actionName,
            BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(method);

        Assert.NotNull(
            method.GetCustomAttribute<HttpPostAttribute>());

        Assert.NotNull(
            method.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>());

        var attributes =
            method.GetCustomAttributes<AuthorizeAttribute>();

        Assert.Contains(
            attributes,
            attribute => attribute.Roles == expectedRole);
    }

    [Theory]
    [InlineData("Index", "Customer")]
    [InlineData("Details", "Customer")]
    [InlineData("Owner", "Owner")]
    [InlineData("OwnerDetails", "Owner")]
    public void GetActions_CoPhanQuyenDung(
        string actionName,
        string expectedRole)
    {
        var controllerType = typeof(BookingsController);

        Assert.NotNull(
            controllerType.GetCustomAttribute<AuthorizeAttribute>());

        var method = controllerType.GetMethod(
            actionName,
            BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(method);

        Assert.NotNull(
            method.GetCustomAttribute<HttpGetAttribute>());

        var attributes =
            method.GetCustomAttributes<AuthorizeAttribute>();

        Assert.Contains(
            attributes,
            attribute => attribute.Roles == expectedRole);
    }
}
