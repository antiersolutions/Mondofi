using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace AIS
{
    /// <summary>
    ///     Extensions making it easier to get the databaseName claims off of an identity
    /// </summary>
    public static class ClaimExtensions
    {
        /// <summary>
        ///     Return the database name using the IdentityClaim
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public static string GetDatabaseName(this IIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            var ci = identity as ClaimsIdentity;
            if (ci != null)
            {
                return ci.FindFirstValue("databaseInfo");
            }
            return null;
        }

        /// <summary>
        ///     Return the company id using the IdentityClaim
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public static string GetCompanyId(this IIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            var ci = identity as ClaimsIdentity;
            if (ci != null)
            {
                return ci.FindFirstValue("companyId");
            }
            return null;
        }

        /// <summary>
        ///     Return the main database user id using the IdentityClaim
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public static string GetTenantUserId(this IIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            var ci = identity as ClaimsIdentity;
            if (ci != null)
            {
                return ci.FindFirstValue("mainDbUserId");
            }
            return null;
        }

        /// <summary>
        ///     Return if user is admin using the IdentityClaim
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public static bool IsAdmin(this IIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            var ci = identity as ClaimsIdentity;
            if (ci != null)
            {
                return Convert.ToBoolean(ci.FindFirstValue("isAdmin"));
            }

            return false;
        }
    }
}
