using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4jClient;
using Neo4jClient.Cypher;
using Neo4jClient.Cypher;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Mappers;
using Neo4jClient.Serialization;
using Neo4jClient;
using Neo4jClient;
using Neo4jClient;
using Neo4jClient;

namespace neo4j.factsheetcode
{
    class Program
    {
        static void Main(string[] args)
        {
            IndexMovieByTitleAndUseIndexForSearch();
            ComplexQuery();
            FindActorsAndRoles();
            CreateActorPlayingInMovie();
            FindMovieByTitle();
            UpdatePropertiesOnActorAndRole();
            DeleteActorAndRoles();

            CodeTeaser();

            // don't do startup/shutdown
            // transactions don't apply here
            CypherStatementWithParameters();
        }

        public static void CreateActorPlayingInMovie()
        {
            var client = new GraphClient(new Uri("http://localhost:7474/db/data"));
            client.Connect();

// wiki up to date


// Create relationship with root node, so we can access the movie.
// Create movie and relationship in one go, so there is only one access to the database
var movie = client.Create(new Movie { Title = "The Matrix" }, new HasMovie(client.RootNode));

var actor = client.Create(new Actor { Name = "Keanu Reeves" }, new ActedIn(movie, new ActedInProperties { Role = "Neo" }));

        }


// ---------------
// Declarations

public class Movie
{
    public string Title { get; set; }
}

public class Actor
{
    public string Name { get; set; }
}

public class ActedIn : Relationship, IRelationshipAllowingSourceNode<Actor>, IRelationshipAllowingTargetNode<Movie>
{
    public ActedIn(NodeReference<Movie> targetNode, ActedInProperties actedInProperties) : base(targetNode, actedInProperties) { }
    public override string RelationshipTypeKey { get { return "ACTED_IN"; } }
}

public class ActedInProperties
{
    public string Role { get; set; }
}

public class HasMovie : Relationship, IRelationshipAllowingSourceNode<RootNode>, IRelationshipAllowingTargetNode<Movie>
{
    public HasMovie(NodeReference<Movie> targetNode): base(targetNode) {}
    public HasMovie(NodeReference<RootNode> rootNode) : base(rootNode) { } 
    public override string RelationshipTypeKey { get { return "HAS_MOVIE"; } }
}


        // ---------------------------------------------------------------------

        public static void FindMovieByTitle()
        {
            var client = new GraphClient(new Uri("http://localhost:7474/db/data"));
            client.Connect();

            // wiki up to date
var movies = client
            .Cypher
            .Start(new { root = client.RootNode })
            .Match("root-[:HAS_MOVIE]->movie")
            .Where((Movie movie) => movie.Title == "The Matrix")
            .Return<Node<Movie>>("movie")
            .Results;
        }

        public static void IndexMovieByTitleAndUseIndexForSearch()
        {
            //#####var movie = client.Create(new Movie { Title = "The Matrix" }, new HasMovie(client.RootNode));

var client = new GraphClient(new Uri("http://localhost:7474/db/data"));
client.Connect();

// index a movie by title

var theMatrix = new Movie { Title = "The Matrix" };

client.Create(
    theMatrix,
    new IRelationshipAllowingParticipantNode<Movie>[0],
    new[]
    {
        new IndexEntry("Movie")
        {
            { "Title", theMatrix.Title }
        }
    });

//  use the index for search

IEnumerable<Node<Movie>> movies = client
                                  .Cypher
                                  .Start(new { movie = Node.ByIndexLookup("Movie", "Title", "The Matrix" )})
                                  .Return<Node<Movie>>("movie")
                                  .Results;
        }

        public static void FindActorsAndRoles()
        {
var client = new GraphClient(new Uri("http://localhost:7474/db/data"));
client.Connect();

// Set up for example
    var movie = client.Create(new Movie { Title = "The Matrix" });

    client.Create(new Actor { Name = "Keanu Reeves" },
        new ActedIn(movie, new ActedInProperties { Role = "Neo" }));

    client.Create(new Actor { Name = "Hugo Weaving" },
        new ActedIn(movie, new ActedInProperties { Role = "Agent Smith" }));

    var actorsAndRoles = client
        .Cypher
        .Start(new { movie = movie })
        .Match("actor-[r:ACTED_IN]->movie")
        .Return((actor, r) => new
        {
            Actor = actor.As<Node<Actor>>() /*,
            Role = r */
        })
        .Results;


//#####                    Role = r.As<Relationship<ActedIn>>()

            //var res = actorsAndRoles
            //    .Results;


        }

        public static void UpdatePropertiesOnActorAndRole()
        {
            var client = new GraphClient(new Uri("http://localhost:7474/db/data"));
            client.Connect();

            // Set up for example
            var movie = client.Create(new Movie { Title = "The Matrix" });
            var actor = client.Create(new Actor { Name = "Keanu Reeves" }, new ActedIn(movie, new ActedInProperties { Role = "Neo" }));

            // wiki up to date
            // Example itself
client.Update(actor, node => { node.Name = "Hugo Weaving"; });
client.Update(movie, node => { node.Title = "The Matrix Reloaded"; });
        }

        public static void DeleteActorAndRoles()
        {
            var client = new GraphClient(new Uri("http://localhost:7474/db/data"));
            client.Connect();

            // Set up for example
            var movie = client.Create(new Movie { Title = "The Matrix" });
            var actor = client.Create(new Actor { Name = "Keanu Reeves" }, new ActedIn(movie, new ActedInProperties { Role = "Neo" }));

            // wiki up to date
            // Example itself
client.Delete(actor, DeleteMode.NodeAndRelationships);
        }


        // --------------------------------

        public static void ComplexQuery()
        {
            var client = new GraphClient(new Uri("http://localhost:7474/db/data"));
            client.Connect();

            // set up data for sample

            var theMatrix = client.Create(new Movie { Title = "The Matrix" });
            var anotherMovie = client.Create(new Movie { Title = "Another movie" });
            var anotherMovie2 = client.Create(new Movie { Title = "Another movie2" });

            // ------

            var keanoReeves = client.Create(new Actor { Name = "Keanu Reeves" },
                new ActedIn(theMatrix, new ActedInProperties { Role = "Neo" }));

            client.CreateRelationship(keanoReeves, new ActedIn(anotherMovie, new ActedInProperties { Role = "Super Hero" }));
            client.CreateRelationship(keanoReeves, new ActedIn(anotherMovie2, new ActedInProperties { Role = "Super Hero2" }));


            // ----

            var hugoWeaving = client.Create(new Actor { Name = "Hugo Weaving" },
                new ActedIn(theMatrix, new ActedInProperties { Role = "Agent Smith" }));

            var hugoWeaving2 = client.Create(new Actor { Name = "Hugo Weaving2" },
                new ActedIn(theMatrix, new ActedInProperties { Role = "Agent Smith2" }));

            var anotherActor = client.Create(new Actor { Name = "Another Actor" },
                new ActedIn(anotherMovie, new ActedInProperties { Role = "Another Role" }));

            // --------------------------
            // Sample itself

// Find all movies that Keano Reeves played in, and his co-actors in each movie
// Cypher pattern:
// keanoReeves-[:ACTED_IN]->movie<-[?:ACTED_IN]-coActor

var moviesAndCoactors = client
    .Cypher
    .Start(new { keanoReeves = keanoReeves })
    .Match("keanoReeves-[:ACTED_IN]->movie<-[?:ACTED_IN]-coActor")
    .Where((Actor coActor) => coActor.Name != "Keanu Reeves")
    .Return((movie, coActor) => new
    {
        Movie = movie.As<Node<Movie>>(),
        CoActor = coActor.As<Node<Actor>>()
    })
    .Results;
        }

        public static void CodeTeaser()
        {
            //TODO:
        }

        public static void CypherStatementWithParameters()
        {
            //TODO:
        }
    }
}
