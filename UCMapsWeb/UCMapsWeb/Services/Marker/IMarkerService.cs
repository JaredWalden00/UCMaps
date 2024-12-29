using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UCMapsWeb.Services.Marker
{
    public interface IMarkerService
    {
        Task InitializeUserIdAsync();
        Task<List<UCMarker>> GetMarkersAsync();
        Task<UCMarker> AddMarkerAsync(UCMarker newMarker);
        Task UpdateMarkerAsync(UCMarker updateMarker);
    }
}
