using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IceCreamSystem.Services
{
    public class Check
    {
        public static bool IsSuperAdmin(int permission)
        {
            return permission == 1;
        }

        public static bool IsAdmin(int permission)
        {
            return permission == 1 || permission == 2;
        }

        public static bool IsSupervisor(int permission)
        {
            return permission == 1 || permission == 2 || permission == 3;
        }
        public static bool IsStockist(int permission)
        {
            return permission == 1 || permission == 2 || permission == 3 || permission == 4;
        }

        public static bool IsSeller(int permission)
        {
            return permission == 1 || permission == 2 || permission == 3 || permission == 5;
        }

        public static bool IsLogOn(int id, int permission, int companyId, string user)
        {
            return id != 0 && permission != 0 && permission != 0 && companyId != 0 && user != null;
        }

        public static bool IsSameCompany(int idCompanyUser, int idCompanyEmployee)
        {
            return idCompanyUser == idCompanyEmployee;
        }

        public static bool IsMe(int idMe, int idEmployee)
        {
            return idMe == idEmployee;
        }
    }
}