using Microsoft.EntityFrameworkCore;
using Reddit;
using Reddit.Models;
using Reddit.Repositories;

namespace UnitTest1
{
    public class UnitTest1
    {

        private IPostsRepository GetPostsRepostory()
        {

            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var dbContext = new ApplicationDbContext(options);

            dbContext.Posts.Add(new Post { Title = "Title 1", Content = "cntnt 1", Upvotes = 25, Downvotes = 15 });
            dbContext.Posts.Add(new Post { Title = "Title 2", Content = "cntnt 1", Upvotes = 1, Downvotes = 51 });
            dbContext.Posts.Add(new Post { Title = "Title 3", Content = "cntnt 1", Upvotes = 5, Downvotes = 11 });
            dbContext.Posts.Add(new Post { Title = "Title 4", Content = "cntnt 1", Upvotes = 1, Downvotes = 123 });
            dbContext.Posts.Add(new Post { Title = "Title 5", Content = "cntnt 1", Upvotes = 15, Downvotes = 1 });
            dbContext.SaveChanges();

            return new PostsRepository(dbContext);
        }

        [Fact]
        public async Task GetPosts_ReturnsCorrectPagination()
        {
            var postsRepository = GetPostsRepostory();
            var posts = await postsRepository.GetPosts(1, 2, null, null, null);

            Assert.Equal(2, posts.Items.Count);
            Assert.Equal(5, posts.TotalCount);
            Assert.True(posts.HasNextPage);
            Assert.False(posts.HasPreviousPage);
        }

        [Fact]
        public async Task GetPosts_ListEmpty_ThrowsArgumentOutOfRangeException()
        {
            var postsRepository = GetPostsRepostory();
            var posts = await postsRepository.GetPosts(1, 2, null, null, null);

            Assert.True(posts.TotalCount == 0);
        }

        [Fact]
        public async Task GetPosts_PageSizeOutOfRange_ThrowsArgumentOutOfRangeException()
        {
            var repository = GetPostsRepostory();
            var posts = await repository.GetPosts(1, 15, null, null, null);

            Assert.True(posts.PageSize > posts.TotalCount);
        }

        [Fact]
        public async Task GetPosts_InvalidTotalCount_ThrowsArgumentOutOfRangeException()
        {
            var postsRepository = GetPostsRepostory();
            var posts = await postsRepository.GetPosts(1, 2, null, null, null);

            Assert.True(posts.TotalCount > posts.PageSize);
        }

        [Fact]
        public async Task GetPosts_InvalidPage_ThrowsArgumentException()
        {
            var repository = GetPostsRepostory();

            var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => repository.GetPosts(0, 10, null, null, null));
            Assert.Equal("page", exception.ParamName);
        }

        [Fact]
        public async Task GetPosts_InvalidPageSize_ThrowsArgumentOutOfRangeException()
        {
            var repository = GetPostsRepostory();

            var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => repository.GetPosts(1, 0, null, null, null));
            Assert.Equal("pageSize", exception.ParamName);
        }

        [Fact]
        public async Task GetPosts_PageIsOutOfRange_ThrowsArgumentOutOfRangeException()
        {
            var repository = GetPostsRepostory();
            var posts = await repository.GetPosts(1000, 2, null, null, null);
            Assert.True(posts.Page > posts.TotalCount);
        }
    }
}