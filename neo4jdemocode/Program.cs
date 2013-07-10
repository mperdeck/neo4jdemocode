using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4jClient;
using Neo4jClient.Cypher;

namespace neo4j.factsheetcode
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateActorPlayingInMovie();
            FindMovieByTitle();
            IndexMovieByTitleAndUseIndexForSearch();
            FindActorsAndRoles();
            UpdatePropertiesOnActorAndRole();
            DeleteActorAndRoles();
            ComplexQuery();

            CodeTeaser();
            StartupShutdown();
            UsingTransactions();
            CypherStatementWithParameters();
        }

        public static void CreateActorPlayingInMovie()
        {
            var client = new GraphClient(new Uri("http://localhost:7474/db/data"));
            client.Connect();

            var movie = client.Create(new Movie { Title = "The Matrix" });
//########            var movie = client.Create(new Movie { Title = "The Matrix" }, new[] { new HasMovie2(client.RootNode) });

            // Create actor and its relationship with the movie in one go, so there is only one access to the database
            var actor = client.Create(new Actor { Name = "Keanu Reeves" }, new ActedIn(movie) { Role = "Neo" });

            // Also add relationship to root node, so we can find movies without going through other nodes
            //TODO: this is inefficient (another call to the db). But I can't add this to the create (movie doesn't yet exist when I call client.Create)
            //TODO: understand this gets created automatically, but that didn't work for me. Confused.
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




        // Reverse HasMovie relationship

        //public class HasMovie2 : Relationship, IRelationshipAllowingSourceNode<Movie>, IRelationshipAllowingTargetNode<RootNode>
        //{
        //    public HasMovie2(NodeReference<RootNode> targetNode)
        //        : base(targetNode)
        //    {
        //    }

        //    public const string TypeKey = "HAS_MOVIE";

        //    public override string RelationshipTypeKey
        //    {
        //        get { return TypeKey; }
        //    }
        //}

        public static void FindMovieByTitle()
        {
            var client = new GraphClient(new Uri("http://localhost:7474/db/data"));
            client.Connect();

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
            //TODO: ???????????? how to create an index on an existing set. How to access it?

            //var client = new GraphClient(new Uri("http://localhost:7474/db/data"));
            //client.Connect();

            //var bookRef = client.Create(book,
            //                new[] { new HasBook(client.RootNode) },
            //                new[] { new IndexEntry("books") { { "title", "p. stone" } } });



            //// Query Index


            //var userNode = client.Cypher
            //                .Start(new
            //                {
            //                    user = Node.ByIndexLookup(IndexNames.UsersKey, UserIndexKeys.Id, id)
            //                })
            //                .Return<Node<UserGraph>>("user")
            //                .Results.FirstOrDefault();

        }

        public static void FindActorsAndRoles()
        {
            //TODO
        }

        public static void UpdatePropertiesOnActorAndRole()
        {
            var client = new GraphClient(new Uri("http://localhost:7474/db/data"));
            client.Connect();

            // Set up for example
            var movie = client.Create(new Movie { Title = "The Matrix" });
            var actor = client.Create(new Actor { Name = "Keanu Reeves" }, new ActedIn(movie) { Role = "Neo" });

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
            var actor = client.Create(new Actor { Name = "Keanu Reeves" }, new ActedIn(movie) { Role = "Neo" });

            // Example itself
            client.Delete(actor, DeleteMode.NodeAndRelationships);
        }

        public static void ComplexQuery()
        {
            //TODO:
        }

        public static void CodeTeaser()
        {
            //TODO:
        }

        private static void StartupShutdown()
        {
            //TODO:
        }

        private static void UsingTransactions()
        {
            //TODO:
        }

        public static void CypherStatementWithParameters()
        {
            //TODO:
        }
    }
}
