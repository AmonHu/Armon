using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace Armon.RESTService.BookServices
{
    /// <summary>
    /// The book data contract class
    /// </summary>
    [DataContract]
    public class Book
    {
        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        /// <value>
        /// The ID.
        /// </value>
        [DataMember]
        public string ID { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        [DataMember]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the author.
        /// </summary>
        /// <value>
        /// The author.
        /// </value>
        [DataMember]
        public string Author { get; set; }

        /// <summary>
        /// Gets or sets the publish year.
        /// </summary>
        /// <value>
        /// The publish year.
        /// </value>
        [DataMember]
        public int PublishYear { get; set; }
    }
}