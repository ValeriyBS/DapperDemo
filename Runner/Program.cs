// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using DataLayer;
using Microsoft.Extensions.Configuration;
using Runner;
//Build a config object, using env vars and JSON providers.
IConfiguration config =  new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

// Get values from the config given their key and their target type.
ConnectionStrings connectionStrings = config.GetRequiredSection("ConnectionStrings").Get<ConnectionStrings>();


//Get_all_should_return_6_results();

//var id = Insert_should_assign_identity_to_new_entity();

//Find_should_retrieve_existing_entity(id);

//Modify_should_update_existing_entity(id);

//Delete_should_remove_entity(id);

//List_support_should_produce_correct_results();

//Dynamic_support_should_produce_correct_results();

//Bulk_insert_should_insert_4_rows();

//GetIllinoisAddresses();

Get_all_should_return_6_results_with_addresses();

Console.WriteLine("Hello, yaml!");
Console.ReadKey();


IContactRepository CreateRepository()
{
    var connStr = config.GetConnectionString("DefaultConnection");
    //return new ContactRepository(connStr);
    //return new ContactRepositoryContrib(connStr);
    return new ContactRepositorySp(connStr);
}

ContactRepositoryEx CreateRepositoryEx()
{
    var connStr = config.GetConnectionString("DefaultConnection");
    return new ContactRepositoryEx(connStr);
}

void Get_all_should_return_6_results()
{
    //arrange
    var repository = CreateRepository();

    //act
    var contacts = repository.GetAll();

    //assert
    Console.WriteLine($"Count: {contacts.Count}");
    Debug.Assert(contacts.Count == 6);
    contacts.Output();
}

int Insert_should_assign_identity_to_new_entity()
{
    // arrange
    IContactRepository repository = CreateRepository();
    var contact = new Contact
    {
        FirstName = "Joe",
        LastName = "Blow",
        Email = "joe.blow@gmail.com",
        Company = "Microsoft",
        Title = "Developer"
    };
    var address = new Address
    {
        AddressType = "Home",
        StreetAddress = "123 Main Street",
        City = "Baltimore",
        StateId = 1,
        PostalCode = "22222"
    };
    contact.Addresses.Add(address);

    // act
    //repository.Add(contact);
    repository.Save(contact);

    // assert
    Debug.Assert(contact.Id != 0);
    Console.WriteLine("*** Contact Inserted ***");
    Console.WriteLine($"New ID: {contact.Id}");
    return contact.Id;
}

void Find_should_retrieve_existing_entity(int id)
{
    // arrange
    IContactRepository repository = CreateRepository();

    // act
    //var contact = repository.Find(id);
    var contact = repository.GetFullContact(id);

    // assert
    Console.WriteLine("*** Get Contact ***");
    contact.Output();
    Debug.Assert(contact.FirstName == "Joe1");
    Debug.Assert(contact.LastName == "Blow1");
    Debug.Assert(contact.Addresses.Count == 1);
    Debug.Assert(contact.Addresses.First().StreetAddress == "123 Main Street");
}

void Modify_should_update_existing_entity(int id)
{
    // arrange
    IContactRepository repository = CreateRepository();

    // act
    //var contact = repository.Find(id);
    var contact = repository.GetFullContact(id);
    contact.FirstName = "Roger";
    contact.LastName = "Wilco";
    contact.Addresses[0].StreetAddress = "456 Main Street";
    //repository.Update(contact);
    repository.Save(contact);

    // create a new repository for verification purposes
    IContactRepository repository2 = CreateRepository();
    //var modifiedContact = repository2.Find(id);
    var modifiedContact = repository2.GetFullContact(id);

    // assert
    Console.WriteLine("*** Contact Modified ***");
    modifiedContact.Output();
    Debug.Assert(modifiedContact.FirstName == "Roger");
    Debug.Assert(modifiedContact.LastName == "Wilco");
    Debug.Assert(modifiedContact.Addresses.First().StreetAddress == "456 Main Street");
}

void Delete_should_remove_entity(int id)
{
    // arrange
    IContactRepository repository = CreateRepository();

    // act
    repository.Remove(id);

    // create a new repository for verification purposes
    IContactRepository repository2 = CreateRepository();
    var deletedEntity = repository2.Find(id);

    // assert
    Debug.Assert(deletedEntity == null);
    Console.WriteLine("*** Contact Deleted ***");
}

void List_support_should_produce_correct_results()
{
    // arrange
    var repository = CreateRepositoryEx();

    // act
    var contacts = repository.GetContactsById(1, 2, 4);

    // assert
    Debug.Assert(contacts.Count == 3);
    contacts.Output();
}

void Dynamic_support_should_produce_correct_results()
{
    // arrange
    var repository = CreateRepositoryEx();

    // act
    var contacts = repository.GetDynamicContactsById(1, 2, 4);

    // assert
    Debug.Assert(contacts.Count == 3);
    contacts.Output();
}

void Bulk_insert_should_insert_4_rows()
{
    // arrange
    var repository = CreateRepositoryEx();
    var contacts = new List<Contact>
    {
        new Contact { FirstName = "Charles", LastName = "Barkley" },
        new Contact { FirstName = "Scottie", LastName = "Pippen" },
        new Contact { FirstName = "Tim", LastName = "Duncan" },
        new Contact { FirstName = "Patrick", LastName = "Ewing" }
    };

    // act
    var rowsAffected = repository.BulkInsertContacts(contacts);

    // assert
    Console.WriteLine($"Rows inserted: {rowsAffected}");
    Debug.Assert(rowsAffected == 4);
}

void GetIllinoisAddresses()
{
    // arrange
    var repository = CreateRepositoryEx();

    // act
    var addresses = repository.GetAddressesByState(17);

    // assert
    Debug.Assert(addresses.Count == 2);
    addresses.Output();
}

async Task Get_all_should_return_6_results_with_addresses()
{
    var repository = CreateRepositoryEx();

    // act
    var contacts = await repository.GetAllContactsWithAddresses();

    // assert
    Console.WriteLine($"Count: {contacts.Count}");
    contacts.Output();
    Debug.Assert(contacts.Count == 6);
    Debug.Assert(contacts.First().Addresses.Count == 2);
}
