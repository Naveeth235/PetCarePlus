// AppointmentsController.cs
// Purpose: API controller for appointment management with role-based endpoints
// Features: Owner CRUD, Admin approve/reject, Vet assigned appointments, proper authorization

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PetCare.Application.Appointments.DTOs;
using PetCare.Application.Common.Interfaces;
using PetCare.Domain.Appointments;
using PetCare.Infrastructure.Auth;

namespace PetCare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // All endpoints require authentication
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly INotificationService _notificationService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AppointmentsController(
        IAppointmentRepository appointmentRepository,
        INotificationService notificationService,
        UserManager<ApplicationUser> userManager)
    {
        _appointmentRepository = appointmentRepository;
        _notificationService = notificationService;
        _userManager = userManager;
    }

    // Sprint Feature: Owner creates appointment request
    [HttpPost]
    [Authorize(Roles = "OWNER,ADMIN")]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // Validate future date
        if (request.RequestedDateTime <= DateTime.Now)
        {
            return BadRequest("Appointment must be scheduled for a future date and time.");
        }
        //create domain entitiy
        var appointment = new Appointment
        {
            PetId = request.PetId,
            OwnerUserId = userId,
            RequestedDateTime = request.RequestedDateTime,
            ReasonForVisit = request.ReasonForVisit,
            Notes = request.Notes,
            Status = AppointmentStatus.Pending
        };
        // call infrastructure to save entity
        var created = await _appointmentRepository.CreateAsync(appointment);
        // Map to DTO and return
        var dto = await MapToDto(created);

        return CreatedAtAction(nameof(GetAppointment), new { id = created.Id }, dto);
    }

    // Sprint Feature: Get appointment by ID (any authenticated user)
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAppointment(Guid id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null)
            return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var userRole = User.FindFirstValue(ClaimTypes.Role);

        // Authorization: Owner can see their own, Admin/Vet can see any
        if (userRole != "ADMIN" && userRole != "VET" && appointment.OwnerUserId != userId)
        {
            return Forbid();
        }

        var dto = await MapToDto(appointment);
        return Ok(dto);
    }

    // Sprint Feature: Owner gets their appointments
    [HttpGet("my")]
    [Authorize(Roles = "OWNER,ADMIN")]
    public async Task<IActionResult> GetMyAppointments()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var appointments = await _appointmentRepository.GetByOwnerUserIdAsync(userId);
        
        // Fixed: Process sequentially to avoid DbContext concurrency issues
        var dtos = new List<AppointmentDto>();
        foreach (var appointment in appointments)
        {
            var dto = await MapToDto(appointment);
            dtos.Add(dto);
        }
        
        return Ok(dtos);
    }

    // Sprint Feature: Admin gets all appointments
    [HttpGet]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> GetAllAppointments()
    {
        var appointments = await _appointmentRepository.GetAllAsync();
        
        // Fixed: Process sequentially to avoid DbContext concurrency issues
        var dtos = new List<AppointmentDto>();
        foreach (var appointment in appointments)
        {
            var dto = await MapToDto(appointment);
            dtos.Add(dto);
        }
        
        return Ok(dtos);
    }

    // Sprint Feature: Admin gets pending appointments for approval
    [HttpGet("pending")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> GetPendingAppointments()
    {
        var appointments = await _appointmentRepository.GetPendingAsync();
        
        // Fixed: Process sequentially to avoid DbContext concurrency issues
        var dtos = new List<AppointmentDto>();
        foreach (var appointment in appointments)
        {
            var dto = await MapToDto(appointment);
            dtos.Add(dto);
        }
        
        return Ok(dtos);
    }

    // Sprint Key Feature: Admin approves/rejects appointments with notifications
    [HttpPut("{id}/status")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> UpdateAppointmentStatus(Guid id, [FromBody] UpdateAppointmentStatusRequest request)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null)
            return NotFound();

        if (appointment.Status != AppointmentStatus.Pending)
        {
            return BadRequest("Only pending appointments can be approved or cancelled.");
        }

        var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        
        // Update appointment
        appointment.Status = request.Status;
        appointment.AdminNotes = request.AdminNotes;
        appointment.UpdatedByUserId = adminUserId;
        
        if (request.Status == AppointmentStatus.Approved)
        {
            appointment.VetUserId = request.VetUserId;
            appointment.ActualDateTime = appointment.RequestedDateTime; // Set confirmed time
        }

        await _appointmentRepository.UpdateAsync(appointment);

        // Sprint Feature: Send notifications based on status change
        if (request.Status == AppointmentStatus.Approved)
        {
            string? vetName = null;
            if (!string.IsNullOrEmpty(request.VetUserId))
            {
                var vet = await _userManager.FindByIdAsync(request.VetUserId);
                vetName = vet?.FullName;
                
                // Notify vet of assignment
                await _notificationService.NotifyVetAssignedAsync(appointment, request.VetUserId);
            }
            
            // Notify owner of approval
            await _notificationService.NotifyAppointmentApprovedAsync(appointment, vetName);
        }
        else if (request.Status == AppointmentStatus.Cancelled)
        {
            // Notify owner of cancellation
            await _notificationService.NotifyAppointmentCancelledAsync(appointment, request.AdminNotes ?? "No reason provided");
        }

        var dto = await MapToDto(appointment);
        return Ok(dto);
    }

    // Sprint Feature: Vet gets assigned appointments
    [HttpGet("assigned")]
    [Authorize(Roles = "VET,ADMIN")]
    public async Task<IActionResult> GetMyAssignedAppointments()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var appointments = await _appointmentRepository.GetByVetUserIdAsync(userId);
        
        // Fixed: Process sequentially to avoid DbContext concurrency issues
        var dtos = new List<AppointmentDto>();
        foreach (var appointment in appointments)
        {
            var dto = await MapToDto(appointment);
            dtos.Add(dto);
        }
        
        return Ok(dtos);
    }

    // Helper method to map domain entity to DTO with user names
    private async Task<AppointmentDto> MapToDto(Appointment appointment)
    {
        var owner = await _userManager.FindByIdAsync(appointment.OwnerUserId);
        ApplicationUser? vet = null;
        
        if (!string.IsNullOrEmpty(appointment.VetUserId))
        {
            vet = await _userManager.FindByIdAsync(appointment.VetUserId);
        }

        return new AppointmentDto
        {
            Id = appointment.Id,
            PetId = appointment.PetId,
            PetName = appointment.Pet?.Name ?? "Unknown Pet",
            OwnerUserId = appointment.OwnerUserId,
            OwnerName = owner?.FullName ?? "Unknown Owner",
            VetUserId = appointment.VetUserId,
            VetName = vet?.FullName,
            RequestedDateTime = appointment.RequestedDateTime,
            ActualDateTime = appointment.ActualDateTime,
            ReasonForVisit = appointment.ReasonForVisit,
            Notes = appointment.Notes,
            AdminNotes = appointment.AdminNotes,
            Status = appointment.Status,
            StatusDisplayName = appointment.StatusDisplayName,
            CreatedAt = appointment.CreatedAt,
            UpdatedAt = appointment.UpdatedAt,
            CanBeCancelled = appointment.CanBeCancelled,
            RequiresAction = appointment.RequiresAction
        };
    }
}
