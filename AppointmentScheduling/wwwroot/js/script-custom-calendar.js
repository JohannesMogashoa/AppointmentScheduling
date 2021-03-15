var routeUrl = location.protocol + "//" + location.host;
var calendar;
$(document).ready(function () {
    $("#appointmentDate").kendoDateTimePicker({
        value: new Date(),
        dateInput: false
    })
    InitializeCalendar();
});

function InitializeCalendar() {
    try {
        var calendarEl = document.getElementById('calendar');
        if (calendarEl != null) {
            calendar = new FullCalendar.Calendar(calendarEl, {
                initialView: 'dayGridMonth',
                headerToolbar: {
                    left: 'prev,next,today',
                    center: 'title',
                    right: 'dayGridMonth,timeGridWeek,timeGridDay'
                },
                selectable: true,
                editable: false,
                select: function (event) {
                    onShowModal(event, null);
                },
                eventDisplay: "block",
                events: function (fetchInfo, successCallBack, failureCallBack) {
                    $.ajax({
                        url: "/api/AppointmentApi/GetCalendarData?doctorId=" + $("#doctorId").val(),
                        type: "GET",
                        dataType: "JSON",
                        success: function (response) {
                            var appointments = [];
                            if (response.status === 1) {
                                $.each(response.dataenum, function (i, data) {
                                    appointments.push({
                                        id: data.id,
                                        title: data.title,
                                        description: data.description,
                                        start: data.startDate,
                                        end: data.endDate,
                                        backgroundColor: data.isDoctorApproved ? "#28a745" : "#dc3545",
                                        borderColor: "#162466",
                                        textColor: "white"
                                    });
                                })
                            }
                            successCallBack(appointments);
                        },
                        error: function (xhr) {
                            console.log(xhr);
                            $.notify("Error", "error");
                        }
                    });
                },
                eventClick: function (info) {
                    getEventDetailsByEventId(info.event);
                }
            });
            calendar.render();
        }
    } catch (e) {
        alert(e);
    }
}

function onShowModal(obj, isEventDetail) {
    if (isEventDetail !== null) {
        $("#id").val(obj.id);
        $("#title").val(obj.title);
        $("#description").val(obj.description);
        $("#appointmentDate").val(obj.startDate);
        $("#duration").val(obj.duration);
        $("#doctorId").val(obj.doctorId);
        $("#patientId").val(obj.patientId);
        $("#lblPatientName").val(obj.patientName);
        $("#lblDoctorName").val(obj.doctorName);
        if (obj.isDoctorApproved) {
            $("#lblStatus").val("Approved");
            $("#btnConfirm").addClass("d-none");
            $("#btnSubmit").addClass("d-none");
        } else {
            $("#lblStatus").val("Pending");
            $("#btnConfirm").removeClass("d-none");
            $("#btnSubmit").removeClass("d-none");
        }
        $("#btnDelete").removeClass("d-none");
    }
    else {
        $("#appointmentDate").val(obj.startStr + " " + new moment().format("hh:mm A"));
        $("#id").val(0);
        $("#btnDelete").addClass("d-none");
        $("#btnSubmit").removeClass("d-none");
    }
    $('#appointmentInput').modal('show')
}

function onCloseModal() {
    $("#appointmentForm")[0].reset();
    $("#id").val(0);
    $("#title").val("");
    $("#description").val("");
    $("#appointmentDate").val("");
    $('#appointmentInput').modal('hide')
}

function onSubmitForm() {
    if (checkValidation()) {
        var requestData = {
            Id: parseInt($("#id").val()),
            Title: $("#title").val(),
            Description: $("#description").val(),
            StartDate: $("#appointmentDate").val(),
            Duration: $("#duration").val(),
            DoctorId: $("#doctorId").val(),
            PatientId: $("#patientId").val()
        };

        $.ajax({
            url: "/api/AppointmentApi/SaveCalendarData",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(requestData),
            success: function (response) {
                if (response.status === 1 || response.status === 2) {
                    calendar.refetchEvents();
                    $.notify(response.message, "success");
                    onCloseModal();
                } else {
                    $.notify(response.message, "error");
                }
            },
            error: function (xhr) {
                console.log(xhr);
                $.notify("Error", "error");
            }
        });
    }
}

function getEventDetailsByEventId(info) {
    $.ajax({
        url: `/api/AppointmentApi/GetCalendarDataById/${info.id}`,
        type: "GET",
        dataType: "JSON",
        success: function (response) {
            if (response.status === 1 && response.dataenum !== undefined) {
                onShowModal(response.dataenum, true);
            }
            successCallBack(appointments);
        },
        error: function (xhr) {
            console.log(xhr);
            $.notify("Error", "error");
        }
    });
}

function checkValidation() {
    var isValid = true;
    if ($("#title").val() === undefined || $("#title").val() === "") {
        isValid = false;
        $("#title").addClass("error");
    } else {
        $("#title").removeClass("error");
    }

    if ($("#appointmentDate").val() === undefined || $("#appointmentDate").val() === "") {
        isValid = false;
        $("#appointmentDate").addClass("error");
    } else {
        $("#appointmentDate").removeClass("error");
    }

    return isValid;
}

function onDoctorChange() {
    calendar.refetchEvents();
}

function onDeleteAppointment() {
    var id = parseInt($("#id").val());
    $.ajax({
        url: `/api/AppointmentApi/DeleteAppointment/${id}`,
        type: "GET",
        dataType: "JSON",
        success: function (response) {
            if (response.status === 1) {
                $.notify(response.message, "success");
                calendar.refetchEvents();
                onCloseModal();
            }
            else {
                $.notify(response.message, "error");
            }
        },
        error: function (xhr) {
            console.log(xhr);
            $.notify("Error", "error");
        }
    });
}

function onConfirm() {
    var id = parseInt($("#id").val());
    $.ajax({
        url: `/api/AppointmentApi/ConfirmEvent/${id}`,
        type: "GET",
        dataType: "JSON",
        success: function (response) {
            if (response.status === 1) {
                $.notify(response.message, "success");
                calendar.refetchEvents();
                onCloseModal();
            }
            else {
                $.notify(response.message, "error");
            }
        },
        error: function (xhr) {
            console.log(xhr);
            $.notify("Error", "error");
        }
    });
}