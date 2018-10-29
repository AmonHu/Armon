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
    /// Book service implementation class
    /// </summary>
    public class BookService : IBookService
    {

        #region IBookService Members

        /// <summary>
        /// Gets all books.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Book> GetAllBooks()
        {
            return BookRepository.GetAllBooks();
        }

        /// <summary>
        /// Gets the book by id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public Book GetBookById(string id)
        {
            return BookRepository.GetBookByID(id);
        }

        /// <summary>
        /// Gets all books.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Book> GetAllBooksInXml()
        {
            return GetAllBooks(); ;
        }

        /// <summary>
        /// Gets the book by id in XML.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public Book GetBookByIdInXml(string id)
        {
            return GetBookById(id);
        }

        /// <summary>
        /// Deletes the book.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public void DeleteBook(string id)
        {
            BookRepository.DeleteBook(id);
        }

        /// <summary>
        /// Updates the book.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="book"></param>
        /// <returns></returns>
        public Book UpdateBook(string id, Book book)
        {
            BookRepository.UpdateBook(id, book);
            return book;
        }

        /// <summary>
        /// Creates the book.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <returns></returns>
        public Book CreateBook(Book book)
        {
            BookRepository.AddBook(book);
            return book;
        }

        #endregion
    }
}
