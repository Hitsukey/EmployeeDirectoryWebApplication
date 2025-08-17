using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EmployeeDirectoryWebApplication.Data;
using EmployeeDirectoryWebApplication.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;


namespace EmployeeDirectoryWebApplication.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly DataContext _context;
        private const int pageSize = 5;

        public EmployeesController(DataContext context)
        {
            _context = context;
        }

        // GET: Employees
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            ViewData["CurrentFilter"] = searchString;

            var employees = _context.Employee.Include(e => e.Department).AsNoTracking();

            if (!string.IsNullOrEmpty(searchString))
            {
                employees = employees.Where(e => (e.LastName + " " + e.FirstName + " " + e.Patronymic).Contains(searchString) || e.PhoneNumber.Contains(searchString));
            }

            var model = await PaginatedList<Employee>.CreateAsync(employees, pageNumber
                ?? 1, pageSize);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_EmployeesTablePartial", model);
            }

            return View(model);
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employee
                .Include(e => e.Department)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            ViewData["DepartmentId"] = new SelectList(_context.Department, "Id", "Name");
            return View();
        }

        // POST: Employees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,LastName,FirstName,Patronymic,DepartmentId,PhoneNumber,ProfilePhoto,ProfilePhotoFile")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                if (employee.ProfilePhotoFile != null && employee.ProfilePhotoFile.Length > 0)
                {
                    employee.ProfilePhoto = await ConvertIFormFileToByteArray(employee.ProfilePhotoFile);
                }

                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DepartmentId"] = new SelectList(_context.Department, "Id", "Name", employee.DepartmentId);
            return View(employee);
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employee.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            ViewData["DepartmentId"] = new SelectList(_context.Department, "Id", "Name", employee.DepartmentId);
            return View(employee);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,LastName,FirstName,Patronymic,DepartmentId,PhoneNumber,ProfilePhoto,ProfilePhotoFile")] Employee employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingEmployee = await _context.Employee.FindAsync(id);

                    if (employee.ProfilePhotoFile != null && employee.ProfilePhotoFile.Length > 0)
                    {
                        existingEmployee.ProfilePhoto = await ConvertIFormFileToByteArray(employee.ProfilePhotoFile);
                    }
                    
                    existingEmployee.LastName = employee.LastName;
                    existingEmployee.FirstName = employee.FirstName;
                    existingEmployee.Patronymic = employee.Patronymic;
                    existingEmployee.DepartmentId = employee.DepartmentId;
                    existingEmployee.PhoneNumber = employee.PhoneNumber;

                    _context.Update(existingEmployee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id))
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
            ViewData["DepartmentId"] = new SelectList(_context.Department, "Id", "Name", employee.DepartmentId);
            return View(employee);
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employee
                .Include(e => e.Department)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employee.FindAsync(id);
            _context.Employee.Remove(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employee.Any(e => e.Id == id);
        }

        private async Task<byte[]> ConvertIFormFileToByteArray(IFormFile photoFile)
        {
            using (var memoryStream = new MemoryStream())
            {
                await photoFile.CopyToAsync(memoryStream);

                using (var image = Image.Load(memoryStream.ToArray()))
                {
                    var size = Math.Min(image.Width, image.Height);
                    var options = new ResizeOptions
                    {
                        Mode = ResizeMode.Crop,
                        Size = new Size(size, size)
                    };

                    image.Mutate(x => x.Resize(options));

                    using (var resultStream = new MemoryStream())
                    {
                        image.Save(resultStream, new JpegEncoder());
                        return resultStream.ToArray();
                    }
                }
            }
        }
    }
}
