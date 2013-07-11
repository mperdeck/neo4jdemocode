using System;
using Neo4jClient;

namespace neo4j.factsheetcode
{
    class ProgramBugReport
    {
        static void MainBugReport(string[] args)
        {
            var client = new GraphClient(new Uri("http://localhost:7474/db/data"));
            client.Connect();

            // set up data for sample

            var theMatrix = client.Create(new Movie { Title = "The Matrix" });
            var anotherMovie = client.Create(new Movie { Title = "Another movie" });

            // ------

            var keanoReeves = client.Create(new Actor { Name = "Keanu Reeves" },
                new ActedIn(theMatrix, new ActedInProperties { Role = "Neo" }));

            client.CreateRelationship(keanoReeves, new ActedIn(anotherMovie, new ActedInProperties { Role = "Super Hero" }));


            // ----

            var hugoWeaving = client.Create(new Actor { Name = "Hugo Weaving" },
                new ActedIn(theMatrix, new ActedInProperties { Role = "Agent Smith" }));

            // --------------------------
            // Sample itself

            // Find all movies that Keano Reeves played in, and his co-actors in each movie
            // Cypher pattern:
            // keanoReeves-[:ACTED_IN]->movie<-[?:ACTED_IN]-coActor

            //var moviesAndCoactors = client
            //    .Cypher
            //    .Start(new { keanoReeves = keanoReeves })
            //    .Match("keanoReeves-[:ACTED_IN]->movie<-[?:ACTED_IN]-coActor")
            //    .Return((movie, coActor) => new
            //    {
            //        Movie = movie.As<Node<Movie>>(),
            //        CoActors = coActor.CollectAs<Node<Actor>>()
            //    })
            //    .Results;

            //var moviesAndCoactors = client
            //    .Cypher
            //    .Start(new { keanoReeves = keanoReeves })
            //    .Match("keanoReeves-[:ACTED_IN]->movie, movie<-[:ACTED_IN]-coActor")
            //    .Return((movie, coActor) => new
            //    {
            //        Movie = movie.As<Node<Movie>>(),
            //        CoActors = coActor.CollectAs<Node<Actor>>()
            //    });

            var moviesAndCoactors = client
                .Cypher
                .Start(new { keanoReeves = keanoReeves })
                .Match("keanoReeves-[:ACTED_IN]->movie, coActor-[:ACTED_IN]->movie")
                .Return((movie, coActor) => new
                {
                    Movie = movie.As<Node<Movie>>(),
                    CoActors = coActor.CollectAs<Node<Actor>>()
                });

            var res = moviesAndCoactors
                .Results;
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
            public HasMovie(NodeReference<Movie> targetNode) : base(targetNode) { }
            public HasMovie(NodeReference<RootNode> rootNode) : base(rootNode) { }
            public override string RelationshipTypeKey { get { return "HAS_MOVIE"; } }
        }
    }
}
