using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contact_Manager_Pro.Models
{
	public class AppUser : IdentityUser
	{
		[Required]
		[Display(Name = "First Name")]
		[StringLength(50, ErrorMessage = "Your {0} cannot be longer than {1} and shorter than {2} characters"), MinLength(2)]
		public string? FirstName { get; set; }

		[Required]
		[Display(Name = "Last Name")]
		[StringLength(50, ErrorMessage = "Your {0} cannot be longer than {1} and shorter than {2} characters"), MinLength(2)]
		public string? LastName { get; set; }

		[NotMapped]
		public string FullName => $"{FirstName} {LastName}";

		// Virtual
		public virtual ICollection<Contact> Contacts { get; set; } = new HashSet<Contact>();
		public virtual ICollection<Category> Categories { get; set; } = new HashSet<Category>();
	}
}
