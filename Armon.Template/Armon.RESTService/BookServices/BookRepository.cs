using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Armon.RESTService.BookServices
{
    public static class BookRepository
    {
        private static List<Book> books = new List<Book>() { new Book(){ ID="1", Title="C# in Depth", Author="Jon Skeet", PublishYear=2010 },
                                                             new Book(){ ID="2", Title="Head First C#", Author="Andrew Stellman and Jennifer Greene", PublishYear=2010},
                                                             new Book(){ ID="3", Title="JavaScript & jQuery:Interactive Front-End Development", Author="Jon Duckett", PublishYear=2012}
                                                            };

        /// <summary>
        /// Gets all books.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Book> GetAllBooks()
        {
            return books.AsEnumerable();
        }

        /// <summary>
        /// Gets the book by ID.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static Book GetBookByID(string id)
        {
            return books.FirstOrDefault(book => book.ID == id);
        }

        /// <summary>
        /// Deletes the book.
        /// </summary>
        /// <param name="id">The id.</param>
        public static void DeleteBook(string id)
        {
            Book bookToDelete = books.FirstOrDefault(b => b.ID == id);
            if (bookToDelete != null)
            {
                books.Remove(bookToDelete);
            }
        }

        /// <summary>
        /// Updates the book.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="book">The book.</param>
        public static void UpdateBook(string id, Book book)
        {
            Book bookToUpdate = books.FirstOrDefault(b => b.ID == id);
            if (bookToUpdate != null)
            {
                book.ID = id;
                books.Remove(bookToUpdate);
                books.Add(book);
            }
        }

        /// <summary>
        /// Adds the book.
        /// </summary>
        /// <param name="book">The book.</param>
        public static void AddBook(Book book)
        {
            Book currentBook = books.FirstOrDefault(b => b.ID == book.ID);
            if (books.FirstOrDefault(b => b.ID == book.ID) == null)
            {
                books.Add(book);
            }
        }
    }
}