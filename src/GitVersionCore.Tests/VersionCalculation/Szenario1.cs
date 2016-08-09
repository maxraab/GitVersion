namespace GitVersionCore.Tests.VersionCalculation
{
    using GitTools;
    using GitTools.Testing;
    using GitVersion;
    using LibGit2Sharp;
    using NUnit.Framework;
    using System.Collections.Generic;

    public class Szenario1
    {
        private Config _config = new Config
        {
            VersioningMode = VersioningMode.ContinuousDelivery,
            Branches = new Dictionary<string, BranchConfig>
                {
                    {
                        "master", new BranchConfig()
                        {
                            Tag = "pre",
                            //Increment = IncrementStrategy.None,
                            //IsDevelop = true,
                        }
                    },
                    {
                        "releases?[/-]", new BranchConfig()
                        {
                            Tag = "rc",
                        }
                    }
                }
        };

        [Test]
        public void CommitsInReleaseBranch()
        {
            using (var fixture = new EmptyRepositoryFixture())
            {
                PrepareRepository(fixture);

                fixture.Repository.CreateBranch("release/0.11");
                fixture.Checkout("release/0.11");

                fixture.AssertFullSemver(_config, "0.11.0-rc.1+0");

                fixture.MakeACommit();

                fixture.AssertFullSemver(_config, "0.11.0-rc.1+1");

                fixture.MakeATaggedCommit("v0.11.0-rc1");

                fixture.AssertFullSemver(_config, "0.11.0-rc.1");

                fixture.MakeACommit();

                fixture.AssertFullSemver(_config, "0.11.0-rc.2+3");

                fixture.MakeACommit();

                fixture.MakeATaggedCommit("v0.11.0");

                fixture.AssertFullSemver(_config, "0.11.0");

                fixture.MakeACommit();
                fixture.MakeACommit();

                fixture.AssertFullSemver(_config, "0.11.1-rc.1+2");

                fixture.Checkout("master");

                fixture.MakeATaggedCommit("0.12.0-pre");

                fixture.AssertFullSemver(_config, "0.12.0-pre");

                fixture.MakeACommit();
                fixture.MakeACommit();

                fixture.AssertFullSemver(_config, "0.12.0-pre+2");
            }
        }

        [Test]
        public void CommitsInReleaseBranchWithNewFeaturesInMaster()
        {
            using (var fixture = new EmptyRepositoryFixture())
            {
                PrepareRepository(fixture);

                fixture.Repository.CreateBranch("release/0.11");
                fixture.Checkout("release/0.11");

                fixture.AssertFullSemver(_config, "0.11.0-rc.1+0");

                fixture.MakeACommit();

                // new Features in Master
                fixture.Checkout("master");
                DevelopFeatureInSeparateBranch(fixture, "BranchA");
                DevelopFeatureInSeparateBranch(fixture, "BranchB");
                fixture.Checkout("release/0.11");

                fixture.AssertFullSemver(_config, "0.11.0-rc.1+1");

                fixture.MakeATaggedCommit("v0.11.0-rc1");

                fixture.AssertFullSemver(_config, "0.11.0-rc.1");

                fixture.MakeACommit();

                fixture.AssertFullSemver(_config, "0.11.0-rc.2+3");

                fixture.MakeACommit();

                fixture.MakeATaggedCommit("v0.11.0");

                fixture.AssertFullSemver(_config, "0.11.0");

                fixture.MakeACommit();
                fixture.MakeACommit();

                fixture.AssertFullSemver(_config, "0.11.1-rc.1+2");

                fixture.Checkout("master");

                fixture.MakeATaggedCommit("0.12.0-pre");

                fixture.AssertFullSemver(_config, "0.12.0-pre");

                fixture.MakeACommit();
                fixture.MakeACommit();

                fixture.AssertFullSemver(_config, "0.12.0-pre+2");
            }
        }

        [Test]
        public void CommitsInMaster()
        {
            using (var fixture = new EmptyRepositoryFixture())
            {
                PrepareRepository(fixture);

                fixture.Checkout("master");

                DevelopFeatureInSeparateBranch(fixture, "BranchA");
                DevelopFeatureInSeparateBranch(fixture, "BranchB");
                DevelopFeatureInSeparateBranch(fixture, "BranchC");

                fixture.AssertFullSemver(_config, "0.11.0-pre+16");
            }
        }

        [Test]
        public void CommitsInFeatureBranchWithSingleFeatureBranch()
        {
            using (var fixture = new EmptyRepositoryFixture())
            {
                PrepareRepository(fixture);

                fixture.Checkout("master");

                DevelopFeatureInSeparateBranch(fixture, "BranchA");
                DevelopFeatureInSeparateBranch(fixture, "BranchB");
                DevelopFeatureInSeparateBranch(fixture, "BranchC");

                fixture.Repository.CreateBranch("BranchD");
                fixture.Checkout("BranchD");
                fixture.MakeACommit();
                fixture.MakeACommit();
                fixture.MakeACommit();
                fixture.MakeACommit();

                fixture.AssertFullSemver(_config, "0.11.0-BranchD.1+23");
            }
        }

        [Test]
        public void CommitsInFeatureBranchWithMultipleFeatureBranches()
        {
            using (var fixture = new EmptyRepositoryFixture())
            {
                PrepareRepository(fixture);

                fixture.Checkout("master");

                DevelopFeatureInSeparateBranch(fixture, "BranchA");
                DevelopFeatureInSeparateBranch(fixture, "BranchB");
                DevelopFeatureInSeparateBranch(fixture, "BranchC");

                fixture.Repository.CreateBranch("BranchD");
                fixture.Checkout("BranchD");
                fixture.MakeACommit();
                fixture.MakeACommit();
                fixture.MakeACommit();
                fixture.MakeACommit();

                fixture.Checkout("master");
                fixture.Repository.CreateBranch("BranchE");
                fixture.Checkout("BranchE");
                fixture.MakeACommit();
                fixture.MakeACommit();
                fixture.MakeACommit();
                fixture.MakeACommit();

                fixture.AssertFullSemver(_config, "0.11.0-BranchE.1+23");
            }
        }

        private void PrepareRepository(EmptyRepositoryFixture fixture)
        {
            fixture.MakeACommit();
            fixture.MakeACommit();
            fixture.MakeACommit();
            fixture.MakeACommit();

            fixture.Repository.CreateBranch("release/0.10");
            fixture.Checkout("release/0.10");

            fixture.MakeACommit();
            fixture.MakeATaggedCommit("v0.10.0");

            fixture.Checkout("master");

            fixture.MakeATaggedCommit("v0.11.0-pre");

            fixture.MakeACommit();
            fixture.MakeACommit();
            fixture.MakeACommit();
            fixture.MakeACommit();
        }

        private void DevelopFeatureInSeparateBranch(EmptyRepositoryFixture fixture, string branchName)
        {
            var currentBranch = fixture.Repository.Head.FriendlyName;

            fixture.Repository.CreateBranch(branchName);
            fixture.Checkout(branchName);
            fixture.MakeACommit();
            fixture.MakeACommit();
            fixture.MakeACommit();
            fixture.MakeACommit();

            fixture.Checkout(currentBranch);

            fixture.Repository.Merge(fixture.Repository.FindBranch(branchName), Generate.SignatureNow());
            fixture.Repository.Branches.Remove(branchName);
        }
    }
}