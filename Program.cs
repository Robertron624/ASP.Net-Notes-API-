using Microsoft.EntityFrameworkCore;

var builder  = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<NoteDb>(options => options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var  app = builder.Build();

if(app.Environment.IsDevelopment()){
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapGet("/", () => "Welcome to Notes API");


// -------------------------------------------ROUTES----------------------------------------

//Get all Notes
app.MapGet("/notes", async (NoteDb db) => await db.Notes.ToListAsync());

//Create new Note 
app.MapPost("/notes/", async (Note n, NoteDb db) => {
    db.Add(n);
    await db.SaveChangesAsync();
    return Results.Created($"/notes/{n.id}", n);
});
//Get Note by Id 
app.MapGet("/notes/{id:int}", async (int id, NoteDb db)=>{
    return await db.Notes.FindAsync(id)
            is Note n
            ? Results.Ok(n)
            : Results.NotFound();
});
//Update Note by Id
app.MapPut("/notes/{id:int}", async (NoteDb db,Note n, int id)=>{
    if(n.id != id){
        return Results.BadRequest();
    }
    var note  = await db.Notes.FindAsync(id);
    if (note is null) return Results.NotFound();    

    note.text = n.text;
    note.done = n.done;
    await db.SaveChangesAsync();
    return Results.Ok(note);
});

//Delete Note by Id
app.MapDelete("/notes/{id:int}", async (NoteDb db, int id)=>{

    var note  = await db.Notes.FindAsync(id);
    if (note is not null){
        db.Notes.Remove(note);
        await db.SaveChangesAsync();
    };    

    return Results.NoContent();
});


// app initiation
await app.RunAsync();


record Note(int id){
    public string text {get;set;} = default!;
    public bool done {get;set;} = default!;
}


// DB Context
class NoteDb: DbContext {
    public NoteDb(DbContextOptions<NoteDb> options): base(options) {

    }
    public DbSet<Note> Notes => Set<Note>();
}