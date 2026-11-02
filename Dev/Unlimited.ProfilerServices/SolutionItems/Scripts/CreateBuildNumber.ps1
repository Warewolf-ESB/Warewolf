$branchName = $env:GIT_BRANCH

Write-Host $branchName
$buildCounter = $env:BUILD_NUMBER

# # This gets the name of the current Git branch. 
 $branch = $branchName -replace '_','-'
 Write-Host $branch

# # Sometimes the branch will be a full path, e.g., 'refs/heads/master'. 
# # If so we'll base our logic just on the last part.
if ($branch.Contains("/")) 
{
  $branch = $branch.substring($branch.lastIndexOf("/")).trim("/")
}
 Write-Host $branch

# #replace numbers with empty strings
 $branch = $branch -replace "[0-9]"

# #first 10 chars
#  $branch = $branch.substring(0, 15)

 $buildNumber = "1.0.${buildCounter}-${branch}"
 
 Write-Host $buildNumber