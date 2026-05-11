using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using QRCodeAttendance.Contract.Entities;
using QRCodeAttendance.Models.Enums;

namespace QRCodeAttendance.Models.DTOs.StudentDto;

public class StudentDto
{
        public Guid StudentId {get; set;}
        public Guid UserId {get; set;}
        public Guid RoleId {get; set;}
        public string? FirstName {get; set;}
        public string? LastName {get; set;}
        public required string FullName {get; set;}
        public string? PhoneNumber {get; set;}
        public string? Address {get; set;}
        public required string Email {get; set;}
        public Gender Gender {get; set;}
        public DateTime DateOfBirth {get; set;}
        public string? PasswordHash {get; set;}
        public string? ConfirmPassword {get; set;}
        public required string MatricNumber {get; set;}
        public StudentLevel StudentLevel {get; set;}
        public Departments Department {get; set;}
        public DateTime CreatedDate {get; set;}
        public DateTime UpdatedDate {get; set;}


}