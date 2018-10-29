using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Web;

namespace Armon.RESTService.BookServices
{

    /// <summary>
    /// Book service contract
    /// </summary>
    [ServiceContract]
    public interface IBookService
    {
        /// <summary>
        /// Gets all books.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebGet(UriTemplate = "", ResponseFormat = WebMessageFormat.Json)]
        IEnumerable<Book> GetAllBooks();

        /// <summary>
        /// Gets all books.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebGet(UriTemplate = "xml", ResponseFormat = WebMessageFormat.Xml)]
        IEnumerable<Book> GetAllBooksInXml();

        /// <summary>
        /// Gets the book by id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        [WebGet(UriTemplate = "{id}", ResponseFormat = WebMessageFormat.Json)]
        Book GetBookById(string id);

        /// <summary>
        /// Gets the book by id in XML.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        [WebGet(UriTemplate = "xml/{id}", ResponseFormat = WebMessageFormat.Xml)]
        Book GetBookByIdInXml(string id);

        /// <summary>
        /// Deletes the book.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        [WebInvoke(UriTemplate = "{id}", Method = "DELETE")]
        void DeleteBook(string id);

        /// <summary>
        /// Updates the book.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        [WebInvoke(UriTemplate = "{id}", Method = "PUT")]
        Book UpdateBook(string id, Book book);


        /// <summary>
        /// Creates the book.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <returns></returns>
        [WebInvoke(UriTemplate = "", Method = "POST")]
        Book CreateBook(Book book);
    }
}
