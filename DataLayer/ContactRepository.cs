using System.Data;
using System.Transactions;
using Dapper;
using Microsoft.Data.SqlClient;

namespace DataLayer
{
    public class ContactRepository : IContactRepository
    {
        private IDbConnection db;

        public ContactRepository(string connString)
        {
            db = new SqlConnection(connString);
        }

        public Contact Find(int id)
        {
            return db.Query<Contact>("select * from Contacts where id =@Id", new { id }).SingleOrDefault();
        }

        public List<Contact> GetAll()
        {
            return db.Query<Contact>("select * from Contacts").ToList();
        }

        public Contact Add(Contact contact)
        {
            var sql = @"insert into Contacts (
                FirstName, LastName, Email, Company, Title)
values(
@FirstName,@LastName, @Email, @Company, @Title); select cast(scope_identity() as int)";

            var id = db.Query<int>(sql, contact).Single();
            contact.Id = id;

            return contact;
        }

        public Contact Update(Contact contact)
        {
            var sql = @"update Contacts
set
FirstName = @FirstName,
LastName = @LastName,
Email = @Email,
Company = @Company,
Title = @Title
where Id = @Id";

            db.Execute(sql, contact);

            return contact;
        }

        public void Remove(int id)
        {
            var sql = @"delete from Contacts where Id=@Id;";
            db.Execute(sql, new{id});
        }

        public Contact GetFullContact(int id)
        {
            var sql =
                "SELECT * FROM Contacts WHERE Id = @Id; " +
                "SELECT * FROM Addresses WHERE ContactId = @Id";

            using var multipleResults = db.QueryMultiple(sql, new { Id = id });
            var contact = multipleResults.Read<Contact>().SingleOrDefault();

            var addresses = multipleResults.Read<Address>().ToList();
            if (contact != null && addresses != null)
            {
                contact.Addresses.AddRange(addresses);
            }

            return contact;
        }

        public void  Save(Contact contact)
        {
            using var txScope = new TransactionScope();

            if (contact.IsNew)
            {
                this.Add(contact);
            }
            else
            {
                this.Update(contact);
            }

            foreach (var addr in contact.Addresses.Where(a => !a.IsDeleted))
            {
                addr.ContactId = contact.Id;

                if (addr.IsNew)
                {
                    this.Add(addr);
                }
                else
                {
                    this.Update(addr);
                }
            }

            foreach (var addr in contact.Addresses.Where(a => a.IsDeleted))
            {
                this.db.Execute("DELETE FROM Addresses WHERE Id = @Id", new { addr.Id });
            }

            txScope.Complete();
        }

        public Address Add(Address address)
        {
            var sql =
                "INSERT INTO Addresses (ContactId, AddressType, StreetAddress, City, StateId, PostalCode) VALUES(@ContactId, @AddressType, @StreetAddress, @City, @StateId, @PostalCode); " +
                "SELECT CAST(SCOPE_IDENTITY() as int)";
            var id = this.db.Query<int>(sql, address).Single();
            address.Id = id;
            return address;
        }

        public Address Update(Address address)
        {
            this.db.Execute("UPDATE Addresses " +
                            "SET AddressType = @AddressType, " +
                            "    StreetAddress = @StreetAddress, " +
                            "    City = @City, " +
                            "    StateId = @StateId, " +
                            "    PostalCode = @PostalCode " +
                            "WHERE Id = @Id", address);
            return address;
        }
    }
}
