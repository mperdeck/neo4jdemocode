using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4jClient;

namespace neo4j.factsheetcode
{
    class Program
    {
        static void Main(string[] args)
        {
 //#######           CreateActorPlayingInMovie();
            FindMovieByTitle();
        }

        public static void CreateActorPlayingInMovie()
        {
            var client = new GraphClient(new Uri("http://localhost:7474/db/data"));
            client.Connect();

            var actor = client.Create(new Actor { Name = "Keanu Reeves" });
            var movie = client.Create(new Movie { Title = "The Matrix" });
            var relationship = client.CreateRelationship(actor, new ActedIn(movie) { Role = "Neo"});

            // Also add relationship to root node, so we can find movies without going through other nodes
            client.CreateRelationship(client.RootNode, new HasMovie(movie));
        }

        public class Movie
        {
            public string Title { get; set; }
        }

        public class Actor
        {
            public string Name { get; set; }
        }

        /// <summary>
        /// Describes an ACTED_IN relationsip, which can only be from an Actor object to a Movie object.
        /// It has one property, Role.
        /// </summary>
        public class ActedIn : Relationship, IRelationshipAllowingSourceNode<Actor>, IRelationshipAllowingTargetNode<Movie>
        {
            public string Role { get; set; }

            public ActedIn(NodeReference<Movie> targetNode)
                : base(targetNode)
            {
            }

            public const string TypeKey = "ACTED_IN";

            public override string RelationshipTypeKey
            {
                get { return TypeKey; }
            }
        }

        /// <summary>
        /// Relationship between the root node and any other node.
        /// Used to keep track of all nodes.
        /// </summary>
        public class HasMovie : Relationship, IRelationshipAllowingSourceNode<RootNode>, IRelationshipAllowingTargetNode<Movie>
        {
            public HasMovie(NodeReference<Movie> targetNode)
                : base(targetNode)
            {
            }

            public const string TypeKey = "HAS_MOVIE";

            public override string RelationshipTypeKey
            {
                get { return TypeKey; }
            }
        }

        public static void FindMovieByTitle()
        {
            var client = new GraphClient(new Uri("http://localhost:7474/db/data"));
            client.Connect();

            //doesn't work
            //var movies = client
            //            .Cypher
            //            .Start(new { root = client.RootNode })
            //            .Match("root-[:HAS_MOVIE]->movie")
            //            .Where((Movie movie) => movie.Title == "{title}")
            //            .WithParam("title", "The Matrix")
            //            .Return<Node<Movie>>("movie")
            //            .Results;

            //works - not using parameters
            var movies = client
                        .Cypher
                        .Start(new { root = client.RootNode })
                        .Match("root-[:HAS_MOVIE]->movie")
                        .Where((Movie movie) => movie.Title == "The Matrix")
                        .Return<Node<Movie>>("movie")
                        .Results;

            // Results factored out to be able to see the query itself
            //var movies = client
            //            .Cypher
            //            .Start(new { root = client.RootNode })
            //            .Match("root-[:HAS_MOVIE]->movie")
            //            .WithParam("title", "The Matrix")
            //            .Where((Movie movie) => movie.Title == "{title}")
            //            .Return<Node<Movie>>("movie");
            //var r = movies.Results;
        }
    }
}
