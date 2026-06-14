using HelloBuddy.Admin.Core.Identity;
using HelloBuddy.Api.Security;
using HelloBuddy.Application.Auth;
using HelloBuddy.Application.Records;

namespace HelloBuddy.Api.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        // All /api/admin endpoints are non-browser service-to-service calls from
        // the server-side UI client. CSRF tokens do not apply at this boundary;
        // protection comes from authenticated caller identity and admin policy.

        app.MapGet("/api/admin/practitioners", async (
            IPractitionerAdminService adminService,
            CancellationToken ct) =>
        {
            var list = await adminService.ListPractitionersAsync(ct);
            return Results.Ok(list);
        }).RequireAuthorization(ApiAuthorizationPolicies.AdminOnly)
          .DisableAntiforgery();

        app.MapPost("/api/admin/practitioners", async (
            AddPractitionerRequest request,
            IPractitionerAdminService adminService,
            CancellationToken ct) =>
        {
            var result = await adminService.AddPractitionerAsync(
                request.FirstName,
                request.LastName,
                request.Email,
                request.PhoneNumber,
                request.Role,
                request.InitialPassword,
                ct);

            return result.Outcome switch
            {
                AdminPractitionerOutcome.Success => Results.Ok(result.Practitioner),
                AdminPractitionerOutcome.EmailAlreadyInUse =>
                    Results.ValidationProblem(new Dictionary<string, string[]>
                    {
                        ["Email"] = [result.Message ?? "Email already in use."]
                    }),
                _ => Results.BadRequest(result.Message)
            };
        }).RequireAuthorization(ApiAuthorizationPolicies.AdminOnly)
          .DisableAntiforgery();

        app.MapPut("/api/admin/practitioners/{id:long}", async (
            long id,
            RenamePractitionerRequest request,
            IPractitionerAdminService adminService,
            CancellationToken ct) =>
        {
            var result = await adminService.RenamePractitionerAsync(
                (ulong)id,
                request.FirstName,
                request.LastName,
                request.Email,
                request.PhoneNumber,
                ct);

            return result.Outcome switch
            {
                AdminPractitionerOutcome.Success => Results.Ok(result.Practitioner),
                AdminPractitionerOutcome.NotFound => Results.NotFound(),
                AdminPractitionerOutcome.EmailAlreadyInUse =>
                    Results.ValidationProblem(new Dictionary<string, string[]>
                    {
                        ["Email"] = [result.Message ?? "Email already in use."]
                    }),
                _ => Results.BadRequest(result.Message)
            };
        }).RequireAuthorization(ApiAuthorizationPolicies.AdminOnly)
          .DisableAntiforgery();

        app.MapPost("/api/admin/practitioners/{id:long}/set-password", async (
            long id,
            SetPasswordRequest request,
            IPractitionerAdminService adminService,
            CancellationToken ct) =>
        {
            var result = await adminService.SetPasswordAsync((ulong)id, request.NewPassword, ct);
            return result.Outcome == AdminPractitionerOutcome.NotFound
                ? Results.NotFound()
                : Results.Ok();
        }).RequireAuthorization(ApiAuthorizationPolicies.AdminOnly)
          .DisableAntiforgery();

        app.MapPost("/api/admin/change-password", async (
            ChangeOwnPasswordRequest request,
            IPractitionerAdminService adminService,
            ICurrentPractitionerAccessor accessor,
            CancellationToken ct) =>
        {
            var result = await adminService.ChangeOwnPasswordAsync(
                accessor.PractitionerId,
                request.CurrentPassword,
                request.NewPassword,
                ct);

            return result.Outcome == AdminPractitionerOutcome.Success
                ? Results.Ok()
                : Results.BadRequest(result.Message ?? "Current password is incorrect.");
        }).RequireAuthorization(ApiAuthorizationPolicies.AdminOnly)
          .DisableAntiforgery();

        app.MapDelete("/api/admin/practitioners/{id:long}", async (
            long id,
            IPractitionerAdminService adminService,
            ICurrentPractitionerAccessor accessor,
            CancellationToken ct) =>
        {
            var result = await adminService.DeactivatePractitionerAsync(
                (ulong)id,
                accessor.PractitionerId,
                ct);

            return result.Outcome switch
            {
                AdminPractitionerOutcome.Success => Results.NoContent(),
                AdminPractitionerOutcome.CannotTargetSelf =>
                    Results.BadRequest(result.Message ?? "Cannot deactivate your own account."),
                _ => Results.NotFound()
            };
        }).RequireAuthorization(ApiAuthorizationPolicies.AdminOnly)
          .DisableAntiforgery();

        return app;
    }
}
