using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using ShikashiAPI;

namespace ShikashiAPI.Migrations
{
    [DbContext(typeof(PersistenceContext))]
    partial class PersistenceContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-rc1-16348");

            modelBuilder.Entity("ShikashiAPI.Model.APIKey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("ExpirationTime");

                    b.Property<string>("Identifier");

                    b.Property<int?>("UserId");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("ShikashiAPI.Model.InviteKey", b =>
                {
                    b.Property<string>("Key");

                    b.HasKey("Key");
                });

            modelBuilder.Entity("ShikashiAPI.Model.UploadedContent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("FileName");

                    b.Property<long>("FileSize");

                    b.Property<string>("MimeType");

                    b.Property<int?>("OwnerId");

                    b.Property<DateTime>("Uploaded")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("UploaderIP");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("ShikashiAPI.Model.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Email");

                    b.Property<string>("Password");

                    b.Property<string>("PasswordSalt");

                    b.HasKey("Id");

                    b.HasAlternateKey("Email");
                });

            modelBuilder.Entity("ShikashiAPI.Model.APIKey", b =>
                {
                    b.HasOne("ShikashiAPI.Model.User")
                        .WithMany()
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("ShikashiAPI.Model.UploadedContent", b =>
                {
                    b.HasOne("ShikashiAPI.Model.User")
                        .WithMany()
                        .HasForeignKey("OwnerId");
                });
        }
    }
}
