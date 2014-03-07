ConstructorMapper
=================

History
-------
*First a word of warning*: ConstructorMapper will not be useful to everybody.  I had a very specific need to address and this is the code that
addressed it.  If it helps someone else, great.  

Our data layer has mutable POCOs coming from the data store, which are being transformed into immutable "clean" objects by a service layer.  This 
tranformation was quick but cumbersome to write, and was often reminescent of:

```
    public User Translate(db_user existing)
    {
        return new User(        
            existing.user_id,
            existing.user_name,
            existing.registration_date . . .
        );
    }
```

These properties could only be passed as constructor parameters because of the immutability constraint of the business layer.  ConstructorMapper seeks
to reduce this friction to allow for convention-based mapping of public properties to constructor parameters, using very quick precompiled expressions.
AutoMapper was considered for this, but was generally found to be very slow -- ConstructorMapper is able to do a million mappings a second on a modern
machine.  As far as I could tell there was no convention to map to constructor parameters, and manually specifying the ConstructUsing wasn't any better 
than the old code.

To use:
-------
* Include ConstructorMapper.cs in your .NET 4.0 project.
* At application start, call ConstructorMapper.Configure for every mapping pair you need
* At runtime, call ConstructorMapper.Map and pass in the original object
 
```
    public void App_Start() 
    {
        ConstructorMapper.Configure<db_user, User>();
    }

    public IEnumerable<User> GetUsers() 
    {
        var users = _db.Fetch<db_user>();
        return users.Select(u => ConstructorMapper.Map<db_user, User>(u));
    }
```