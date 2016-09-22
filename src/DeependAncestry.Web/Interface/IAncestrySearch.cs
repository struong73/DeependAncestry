using DeependAncestry.Web.Models;
using System;
using System.Collections.Generic;

namespace DeependAncestry.Web.Interface
{
    public interface IAncestrySearch
    {
        List<Results> Search(string name, string gender, string direction);
    }
}
