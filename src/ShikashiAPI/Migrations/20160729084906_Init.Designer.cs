using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using ShikashiAPI;

namespace ShikashiAPI.Migrations
{
    [DbContext(typeof(PersistenceContext))]
    [Migration("20160729084906_Init")]
    partial class Init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431");

            modelBuilder.Entity("ShikashiAPI.Model.APIKey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("ExpirationTime");

                    b.Property<string>("Identifier");

                    b.Property<int?>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("APIKey");
                });

            modelBuilder.Entity("ShikashiAPI.Model.InviteKey", b =>
                {
                    b.Property<string>("Key");

                    b.HasKey("Key");

                    b.ToTable("InviteKey");
                });

            modelBuilder.Entity("ShikashiAPI.Model.UploadedContent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("FileName");

                    b.Property<long>("FileSize");

                    b.Property<string>("MimeType");

                    b.Property<int?>("OwnerId");

                    b.Property<DateTime>("Uploaded");

                    b.Property<string>("UploaderIP");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("UploadedContent");
                });

            modelBuilder.Entity("ShikashiAPI.Model.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Email")
                        .IsRequired();

                    b.Property<string>("Password");

                    b.Property<string>("PasswordSalt");

                    b.HasKey("Id");

                    b.HasAlternateKey("Email");

                    b.ToTable("User");
                });

            modelBuilder.Entity("ShikashiAPI.Model.APIKey", b =>
                {
                    b.HasOne("ShikashiAPI.Model.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("ShikashiAPI.Model.UploadedContent", b =>
                {
                    b.HasOne("ShikashiAPI.Model.User", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId");
                });
        }
    }
}
