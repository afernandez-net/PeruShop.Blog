using Blog.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Services
{
    public class DbBlogService : IBlogService
    {
        private readonly BlogContext context;
        private readonly IHttpContextAccessor contextAccessor;

        public DbBlogService(BlogContext context, IHttpContextAccessor contextAccessor)
        {
            this.context = context;
            this.contextAccessor = contextAccessor;
        }

        public async Task DeletePost(Post post)
        {
            context.Posts.Remove(post);
            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<string>> GetCategories()
        {
            bool isAdmin = IsAdmin();

            var categories = await context.Posts
                .Where(p => p.IsPublished || isAdmin)
                .SelectMany(post => post.Categories)
                .Select(cat => cat.ToLowerInvariant())
                .Distinct().ToListAsync();

            return categories;
        }

        public Task<Post> GetPostById(string id)
        {
            var post = context.Posts
                .Include(x => x.Comments)
                .FirstOrDefault(p => p.Id.Equals(id));

            bool isAdmin = IsAdmin();

            if (post != null && post.PubDate <= DateTime.UtcNow && (post.IsPublished || isAdmin))
            {
                return Task.FromResult(post);
            }

            return Task.FromResult<Post>(null);
        }

        public Task<Post> GetPostBySlug(string slug)
        {
            var post = context.Posts
                .Include(x => x.Comments)
                .FirstOrDefault(p => p.Slug.Equals(slug));

            bool isAdmin = IsAdmin();

            if (post != null && post.PubDate <= DateTime.UtcNow && (post.IsPublished || isAdmin))
            {
                return Task.FromResult(post);
            }

            return Task.FromResult<Post>(null);
        }

        public async Task<IEnumerable<Post>> GetPosts(int count, int skip = 0)
        {
            bool isAdmin = IsAdmin();

            var posts = await context.Posts
                .Include(x => x.Comments)
                .Where(p => p.PubDate <= DateTime.UtcNow && (p.IsPublished || isAdmin))
                .Skip(skip)
                .Take(count).ToListAsync();

            return posts;
        }

        public async Task<IEnumerable<Post>> GetPostsByCategory(string category)
        {
            bool isAdmin = IsAdmin();

            var posts = await context.Posts
                .Where(p => p.PubDate <= DateTime.UtcNow && (p.IsPublished || isAdmin))
                .ToListAsync();

            posts = posts.Where(p => p.Categories.Contains(category, StringComparer.OrdinalIgnoreCase)).ToList();

            return posts;
        }

        public Task<string> SaveFile(byte[] bytes, string fileName, string suffix = null)
        {
            throw new NotImplementedException();
        }

        public async Task SavePost(Post post)
        {
            if (await GetPostById(post.Id) == null)
                context.Posts.Add(post);
            else
                context.Entry<Post>(post).State = EntityState.Modified;

            await context.SaveChangesAsync();
        }

        protected bool IsAdmin()
        {
            return contextAccessor.HttpContext?.User?.Identity.IsAuthenticated == true;
        }
    }
}