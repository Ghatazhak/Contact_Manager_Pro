using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Contact_Manager_Pro.Data;
using Contact_Manager_Pro.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Contact_Manager_Pro.Enums;
using Contact_Manager_Pro.Models.ViewModels;
using Contact_Manager_Pro.Services.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Contact_Manager_Pro.Controllers
{
    [Authorize]
    public class ContactsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IImageService _imageService;
        private readonly IAddressBookService _addressBookService;
        private readonly IEmailSender _emailService;

        public ContactsController(ApplicationDbContext context, UserManager<AppUser> userManager, IImageService imageService, IAddressBookService addressBookService, IEmailSender emailService)
        {
            _context = context;
            _userManager = userManager;
            _imageService = imageService;
            _addressBookService = addressBookService;
            _emailService = emailService;
        }

        // GET: Contacts
        [Authorize]
        public IActionResult Index(int categoryId, string swalMessage = null)
        {
            ViewData["SwalMessage"] = swalMessage;

            // Create an empy list than can hold contacts
            var contacts = new List<Contact>();

            // Get the appUserId of the current logged in user
            string appUserId = _userManager.GetUserId(User);

            //return the userId and its associated contacts and categories, Joins the tables of categories and contacts based on the user Id.
            AppUser? appUser = _context.Users
                .Include(c => c.Contacts)
                .ThenInclude(c => c.Categories)
                .FirstOrDefault(u => u.Id == appUserId);

            // Get the categories from the join table.
            var categories = appUser!.Categories;

            if (categoryId == 0)
            {
                contacts = appUser.Contacts.OrderBy(c => c.LastName)
                                            .ThenBy(c => c.FirstName)
                                            .ToList();
            }
            else
            {
                contacts = appUser.Categories.FirstOrDefault(c => c.Id == categoryId)!
                                  .Contacts
                                  .OrderBy(c => c.LastName)
                                  .ThenBy(c => c.FirstName)
                                  .ToList();
            }

            // Setup the select list for the view. The last argument categoryId preselects the selection list with request categoryid
            ViewData["CategoryId"] = new SelectList(categories, "Id", "Name", categoryId);

            // return the contacts from the join table.
            return View(contacts);
        }

        [Authorize]
        public IActionResult SearchContacts(string searchString)
        {
            string appUserId = _userManager.GetUserId(User);
            List<Contact> contacts = new List<Contact>();

            AppUser appUser = _context.Users
                .Include(c => c.Contacts)
                .ThenInclude(c => c.Categories)
                .FirstOrDefault(u => u.Id == appUserId)!;

            if (String.IsNullOrEmpty(searchString))
            {
                contacts = appUser.Contacts
                                    .OrderBy(c => c.LastName)
                                    .ThenBy(c => c.FirstName)
                                    .ToList();

            }
            else
            {
                contacts = appUser.Contacts.Where(c => c.FullName!.ToLower().Contains(searchString.ToLower()))
                                   .OrderBy(c => c.LastName)
                                   .ThenBy(c => c.FirstName)
                                   .ToList();
            }

            ViewData["CategoryId"] = new SelectList(appUser.Categories, "Id", "Name", 0);

            return View(nameof(Index), contacts);
        }

        [Authorize]
        public async Task<IActionResult> EmailContact(int id)
        {
            string appUserId = _userManager.GetUserId(User);
            Contact? contact = await _context.Contacts.Where(c => c.Id == id && c.AppUserId == appUserId)
                .FirstOrDefaultAsync();

            if (contact == null)
            {
                return NotFound();
            }

            EmailData emailData = new EmailData()
            {
                EmailAddress = contact.Email!,
                FirstName = contact.FirstName,
                LastName = contact.LastName,
            };

            EmailContactViewModel model = new EmailContactViewModel()
            {
                EmailData = emailData,
                Contact = contact

            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EmailContact(EmailContactViewModel ecvm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _emailService.SendEmailAsync(ecvm.EmailData!.EmailAddress, ecvm.EmailData.Subject,
                        ecvm.EmailData.Body);

                    return RedirectToAction("Index", "Contacts", new { swalMessage = "Success: Email Sent!" });
                }
                catch (Exception e)
                {
                    return RedirectToAction("Index", "Contacts", new { swalMessage = "Error: Email Send Failed!" });
                    Console.WriteLine(e.Message);
                    throw;
                }
            }

            return View();
        }


        // GET: Contacts/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Contacts == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .Include(c => c.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // GET: Contacts/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            string appUserId = _userManager.GetUserId(User);
            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(USStates)).Cast<USStates>().ToList());
            ViewData["CategoryList"] = new MultiSelectList(await _addressBookService.GetUserCategoriesAsync(appUserId), "Id", "Name");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,BirthDate,Address,Address2,City,State,ZipCode,Email,PhoneNumber,ImageFile,")] Contact contact, List<int> CategoryList)
        {
            // Remove AppUserId from the model state check. This will be useful.
            ModelState.Remove("AppUserId");

            if (ModelState.IsValid)
            {

                // Get current logged in user id.
                contact.AppUserId = _userManager.GetUserId(User);

                // This may be the work around for Postgres
                contact.Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);

                if (contact.BirthDate != null)
                {
                    // Casting a standard DateTime into a utc datetime. Again Postgres fix.
                    contact.BirthDate = DateTime.SpecifyKind(contact.BirthDate.Value, DateTimeKind.Utc);
                }
                if (contact.ImageFile != null)
                {
                    // set the incoming IFormFile to convert to ImageData and save to contact
                    contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                    // get the incoming IFormFile contenttype and saving it in the image type.
                    contact.ImageType = contact.ImageFile.ContentType;
                }

                _context.Add(contact);
                await _context.SaveChangesAsync();

                // Loop over all the selected categories.
                foreach (int categoryId in CategoryList)
                {
                    //Save each category selected to the contactcategories table.
                    await _addressBookService.AddContactToCategoryAsync(categoryId, contact.Id);
                }

                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Contacts/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Contacts == null)
            {
                return NotFound();
            }

            // Checks to make sure you are requesting a contact that belongs to the current logged in user.
            string appUserId = _userManager.GetUserId(User);
            var contact = await _context.Contacts.Where(c => c.Id == id && c.AppUserId == appUserId)
                                                .FirstOrDefaultAsync();

            if (contact == null)
            {
                return NotFound();
            }

            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(USStates)).Cast<USStates>().ToList());
            ViewData["CategoryList"] = new MultiSelectList(await _addressBookService.GetUserCategoriesAsync(appUserId), "Id", "Name", await _addressBookService.GetContactCategoryIdsAsync(contact.Id));
            return View(contact);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AppUserId,FirstName,LastName,BirthDate,Address,Address2,City,State,ZipCode,Email,PhoneNumber,Created,ImageData,ImageType,ImageFile, Categories")] Contact contact, List<int> CategoryList)
        {


            if (id != contact.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Postgres datetime fix
                    contact.Created = DateTime.SpecifyKind(contact.Created, DateTimeKind.Utc);

                    if (contact.BirthDate != null)
                    {
                        // Postgres datetime fix had to all Value on this because it was coming from the form.
                        contact.BirthDate = DateTime.SpecifyKind(contact.BirthDate.Value, DateTimeKind.Utc);
                    }

                    if (contact.ImageFile != null)
                    {
                        contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                        contact.ImageType = contact.ImageFile.ContentType;
                    }


                    _context.Update(contact);
                    await _context.SaveChangesAsync();

                    // Had to add extra parens to allow ToList(); to function not sure why
                    // Remove current categories
                    List<Category> oldCategories =
                        (await _addressBookService.GetContactCategoriesAsync(contact.Id)).ToList();
                    foreach (var category in oldCategories)
                    {
                        await _addressBookService.RemoveContactFromCategoryAsync(category.Id, contact.Id);
                    }

                    // Add the currently selected categories
                    foreach (var categoryId in CategoryList)
                    {
                        await _addressBookService.AddContactToCategoryAsync(categoryId, contact.Id);
                    }

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContactExists(contact.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "Id", contact.AppUserId);
            return View(contact);
        }

        // GET: Contacts/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Contacts == null)
            {
                return NotFound();
            }

            string appUserId = _userManager.GetUserId(User);

            var contact = await _context.Contacts
                .FirstOrDefaultAsync(c => c.Id == id && c.AppUserId == appUserId);

            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // POST: Contacts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {

            string appUserId = _userManager.GetUserId(User);

            var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.Id == id && c.AppUserId == appUserId);

            if (contact != null)
            {
                _context.Contacts.Remove(contact);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ContactExists(int id)
        {
            return _context.Contacts.Any(e => e.Id == id);
        }
    }
}
