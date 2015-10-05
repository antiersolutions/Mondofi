using System;
using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web;
using AISModels;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Data.Entity.ModelConfiguration.Conventions;
using AIS.Helpers;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace AIS.Models
{
    //public class UsersContext : DbContext
    //{
    //    public UsersContext()
    //        : base("DefaultConnection")
    //    {
    //    }
    //public class UsersContext : IdentityDbContext<UserProfile>
    public class SAASContext : IdentityDbContext<UserProfile, AISRole, long, AISUserLogin, AISUserRole, AISUserClaim>
    {
        public SAASContext()
            : base("DefaultConnection")
        {

        }

        public static SAASContext Create()
        {
            return new SAASContext();
        }
        public DbSet<Address> tabAddress { get; set; }
        public DbSet<Country> tabCountry { get; set; }

    }

    public class UsersContext : IdentityDbContext<UserProfile, AISRole, long, AISUserLogin, AISUserRole, AISUserClaim>
    {

        public UsersContext()
            : base(CommonHelper.getCurrentUserConnectionString())
        {

        }


        public UsersContext(string dbname)
            : base(CommonHelper.getCurrentUserConnectionString(dbname))
        {

        }
        //public static UsersContext Create()
        //{
        //    return new UsersContext();
        //}

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

            base.OnModelCreating(modelBuilder);
        }

        #region Account Sets

        //public DbSet<UserProfile> Users { get; set; }
        public DbSet<UserPhones> tabUserPhones { get; set; }

        #endregion

        #region Master Sets

        public DbSet<Allergies> tabAllergies { get; set; }
        public DbSet<Cities> tabCities { get; set; }
        public DbSet<PhoneTypes> tabPhoneTypes { get; set; }
        public DbSet<Restrictions> tabRestrictions { get; set; }
        public DbSet<SpecialStatus> tabSpecialStatus { get; set; }
        public DbSet<EmailTypes> tabEmailTypes { get; set; }
        public DbSet<Designations> tabDesignations { get; set; }

        #endregion

        #region Customer Sets

        public DbSet<Customers> tabCustomers { get; set; }
        public DbSet<CustomersAllergies> tabCustomersAllergies { get; set; }
        public DbSet<CustomersEmails> tabCustomersEmails { get; set; }
        public DbSet<CustomerSpecialStatus> tabCustomerSpecialStatus { get; set; }
        public DbSet<CustomersPhoneNumbers> tabCustomersPhoneNumbers { get; set; }
        public DbSet<CustomersRestrictions> tabCustomersRestrictions { get; set; }

        #endregion

        #region Menu Shift Sets

        public DbSet<FoodMenuShift> tabFoodMenuShift { get; set; }
        public DbSet<WeekDays> tabWeekDays { get; set; }
        public DbSet<MenuShiftHours> tabMenuShiftHours { get; set; }

        #endregion

        #region Floor Sets

        public DbSet<TempFloorPlan> tabTempFloorPlans { get; set; }
        public DbSet<TempFloorTable> tabTempFloorTables { get; set; }
        public DbSet<Section> tabSections { get; set; }
        public DbSet<FloorPlan> tabFloorPlans { get; set; }
        public DbSet<FloorTable> tabFloorTables { get; set; }
        public DbSet<FloorTableBlock> tabFloorTableBlocks { get; set; }
        public DbSet<FloorTimeSetting> tabFloorTimeSettings { get; set; }
        public DbSet<ShiftNotes> ShiftNotes { get; set; }
        public DbSet<MergedFloorTable> tabMergedFloorTables { get; set; }
        public DbSet<MergedTableOrigionalTable> tabMergedTableOrigionalTables { get; set; }

        #endregion

        #region Reservation Sets

        public DbSet<Reservation> tabReservations { get; set; }
        public DbSet<Status> Status { get; set; }
        public DbSet<ReservationAudit> tabReservationAudits { get; set; }

        #endregion

        #region Table Availability Sets

        public DbSet<TableAvailability> tabTableAvailabilities { get; set; }
        public DbSet<TableAvailabilityFloorTable> tabTableAvailabilityFloorTables { get; set; }
        public DbSet<TableAvailabilityWeekDay> tabTableAvailabilityWeekDays { get; set; }
        public DbSet<AvailablityStatus> tabAvailablityStatus { get; set; }

        #endregion

        #region Waiting Sets

        public DbSet<Waiting> tabWaitings { get; set; }

        #endregion

        #region Staff

        public DbSet<FloorTableServer> tabFloorTableServers { get; set; }
        public DbSet<ReservationServer> tabReservationServers { get; set; }

        #endregion

        #region Email

        public DbSet<MessageTemplate> tabMessageTemplates { get; set; }

        #endregion

        #region Settings

        public DbSet<Setting> tabSettings { get; set; }

        #endregion
    }

    //[Table("UserProfile")]
    public class UserProfile : IdentityUser<long, AISUserLogin, AISUserRole, AISUserClaim>
    // public class UserProfile : IdentityUser
    {
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public override long Id { get; set; }
        public override string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public int? DesignationId { get; set; }
        //[ForeignKey("DesignationId")]
        public virtual Designations Designation { get; set; }

        public string PhotoPath { get; set; }
        public string StaffColor { get; set; }
        public bool? Availability { get; set; }
        public int UserCode { get; set; }
        public bool EnablePIN { get; set; }
        public string RestaurantName { get; set; }
        public bool? TermAndCondition { get; set; }
        public bool? Approved { get; set; }
        public string Notes { get; set; }
        public long? AddressId { get; set; }
        public virtual Address Address { get; set; }

        public virtual ICollection<UserPhones> PhoneNumbers { get; set; }
        public virtual ICollection<FloorTableServer> ServingTables { get; set; }
        public virtual ICollection<ReservationServer> ServingReservations { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<UserProfile, long> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here

            userIdentity.AddClaim(new Claim("isAdmin", manager.IsInRole(this.Id, "Admin").ToString()));

            using (var mainDB = new SAASContext())
            {
                var user = mainDB.Users.Where(u => u.UserName == this.UserName).SingleOrDefault();
                string databasename = user.RestaurantName;
                databasename = databasename.Replace(" ", "");
                if (user != null)
                {
                    userIdentity.AddClaim(new Claim("databaseInfo", databasename));
                    userIdentity.AddClaim(new Claim("companyId", databasename));
                    userIdentity.AddClaim(new Claim("mainDbUserId", databasename));
                }
            }

            return userIdentity;
        }
    }

    public class AISUserLogin : IdentityUserLogin<long> { }

    public class AISUserRole : IdentityUserRole<long> { }

    public class AISUserClaim : IdentityUserClaim<long> { }

    public class AISRole : IdentityRole<long, AISUserRole> { }


    public class RegisterExternalLoginModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        public string ExternalLoginData { get; set; }
    }

    public class LocalPasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 7)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class ForgotPasswordModel
    {
        [Required]
        [Display(Name = "User name")]
        [Remote("IsEmailExist", "Account")]
        public string UserName { get; set; }
    }

    public class RegisterModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 7)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
    public class RegisterViewModel
    {
        //[Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Email address is not valid")]
        [Display(Name = "Email")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
       
        [Display(Name = "Surname")]
        public string LastName { get; set; }


        [Required]
        [Display(Name = "Venue Name")]
        public string RestaurantName { get; set; }



        [Required]
        [Display(Name = "Venue Phone #")]
        public string phone { get; set; }


        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 7)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Terms & Conditions")]
        public bool TermAndCondition { get; set; }
        public bool Approved { get; set; }

        public string Notes { get; set; }

        public long AddressId { get; set; }
        public virtual Address Address { get; set; }

       
        
    }

    public class ExternalLogin
    {
        public string Provider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderUserId { get; set; }
    }
}
