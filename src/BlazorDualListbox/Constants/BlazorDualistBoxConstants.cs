using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorDualListbox.Constants
{
    /// <summary>
    /// Contains constant values used in the BlazorDualListbox component.
    /// </summary>
    public class BlazorDualistBoxConstants
    {
        /// <summary>
        /// The default caption for the source list header.
        /// </summary>
        public const string DefaultSourceHeaderCaption = "Available";
        /// <summary>
        /// The default caption for the selected list header.
        /// </summary>
        public const string DefaultSelectedHeaderCaption = "Selected";
        /// <summary>
        /// The default text for the button that adds a single item.
        /// </summary>
        public const string DefaultAddSingleItemToSelectedButtonText = "›";
        /// <summary>
        /// The default text for the button that adds all items.
        /// </summary>
        public const string DefaultAddAllToSelectedButtonText = "»";
        /// <summary>
        /// The default text for the button that removes a single item.
        /// </summary>
        public const string DefaultRemoveSingleItemFromSelectedButtonText = "‹";
        /// <summary>
        /// The default text for the button that removes all items.
        /// </summary>
        public const string DefaultRemoveAllFromSelectedButtonText = "«";
    }
}
