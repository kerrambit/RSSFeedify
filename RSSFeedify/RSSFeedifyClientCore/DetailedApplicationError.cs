using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSSFeedifyClientCore
{
    public record DetailedApplicationError(ApplicationError Error, string Details);
}
