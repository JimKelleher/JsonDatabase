using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace Database
{
    public class JsonPath
    {
        // Utility to generate C# classes from JSON:
        // http://json2csharp.com/

        // NOTE: XPath (XML Path Language) is a query language for selecting nodes from an XML document.
        // JSONPath is a JSON implementation of XPath created by Stefan Goessner.  James Newton-King
        // of Newtonsoft implemented JSONPath in C# in the Json.NET library shipped by Microsoft.
        // Json.NET's SelectToken makes dynamic queries easy because the entire query is defined
        // in a string.  This was my reason for selecting it as my ad hoc query method.

        // XPath Tutorial:
        // https://www.w3schools.com/xml/xpath_intro.asp

        // XPath Examples:
        // https://msdn.microsoft.com/en-us/library/ms256086(v=vs.110).aspx

        // Stefan Goessner's JSONPath: XPath for JSON:
        // http://goessner.net/articles/JsonPath/

        // Newtonsoft's Json.NET Documentation:
        // http://www.newtonsoft.com/json/help/html/Introduction.htm

        // Json.NET: Querying JSON with SelectToken:
        // http://www.newtonsoft.com/json/help/html/SelectToken.htm

        //---------------------------------------------------------------------------------------------------------
        // DESIGN NOTES:
        // The function of this process is to do general-purpose JSONPath querying.  The great difficulty of
        // this task is to handle the "nesting" of JSON data-structures by the use of arrays as follows: 

        // { "Discography":[                                    group level 1       index 0
        //       { "sections":[                                 group level 2       index 1
        //             { "albums":[                             group level 3       index 2
        //                   { "album":"Album 0, 0, 0" }        detail level
        //                ]
        //             }
        //          ]
        //       }
        //    ]
        // }

        // DESIGN CONSTRAINT ONE - My Design
        //
        // NOTE: JSON data can also be nested using curly-brace-based data collections similar to old COBOL
        // "group levels".  My research tells me that they are not as problematic but I have none in my
        // systems and did not attempt to work with them.
        //
        // I envisioned building this process in three stages:
        //
        // 1) Build using loop-based iteration employing a hardcoded maximum of three levels (as above).
        // 2) Modify the Step 1 product to handle fewer levels (one or two), ie, "downsize".
        // 3) Modify the Step 2 product to remove the level limit hardcode, probably employing recursion
        //    in place of loops.
        //
        // The first step was completed in approximately one month's time.  I now have a functioning mechanism
        // that handles my deepest JSON data structures, none of which is deeper than three group levels.  I
        // chose not to go ahead with Step 3 but have left a blueprint for how to do so.
        //
        // The one month of development got me a three level data handler.  Only two days were required to make
        // this handle 1 or 2 levels.  In both cases, my usage of hardcoded index numbers was the key to finding
        // where the rework needed to take place.  The final changes needed to make this happen are particulary
        // kludgy in appearance and are probably the key to a recursive rework (see "LEVEL HARDCODE" and "DOWNSIZE
        // KLUDGE" code markers).

        // DESIGN CONSTRAINT TWO - JSONPath
        //
        // The following are JSONPath query strings.  As you can see, they carry the level name (Discography,
        // sections, albums) as the highest level qualifier.

        // $..Discography[?(@.id      == 'Artist 0'      )].id
        // $..sections[   ?(@.section == 'Section 0, 0'  )].section
        // $..albums[     ?(@.album   == 'Album 0, 0, 0' )].album

        // Queries can combine lookups WITHIN a level, eg: (@.id == 'Artist 0' || @.id == 'Artist 1') but not
        // ACROSS levels, eg: (@.id == 'Artist 0' || @.section == 'Section 0, 0').  I envisioned a solution
        // to this problem:  Multiple JSONPath queries would be necessary with the output from each "filter"
        // being the input to the next.
        //
        // Thus, two entirely different methods would be needed to do the same thing.  As an expedient, all
        // queries could be done with the second method but, regardless, much work lies ahead to accomplish
        // this simple goal.
        //
        // Since my data structures have not much need for cross-level combined queries and since this project
        // is way over schedule, I chose not to go ahead with this enhancement.  Enough is enough.

        // DESIGN CONSTRAINT THREE - JSONPath
        //
        // Sadly, JSONPath does not allow partial matching on string fields, simlar to SQL LIKE.  This severely
        // limits its usefulness.  To get this functionality I would have had to 1) employ another third party
        // query tool or 2) write it myself.
        //---------------------------------------------------------------------------------------------------------

        // Used by InputCriteria() and ExecuteJsonPathQuery():
        public string entity;

        // Used by GetJsonGroups() and InputCriteria():
        public string fullDatabase;

        // Used by AssembleQueryPathString(), GetExtrapolatedPathIndices() and PathFound():
        JObject jObjectFullDatabase;

        // Used by: AssembleQueryPathString(), PathExtrapolationPrep(), PathExtrapolation() and QueryJsonPathMain():
        List<int[]> pathLevelIndicesParent = new List<int[]>();

        // Used by: AssembleQueryPathString(), EndOfSection(), GetExtrapolatedPathIndices() and GetJsonGroups():
        List<KeyValuePair<string, string>> arrayKeyValuePairAllSections = new List<KeyValuePair<string, string>>();

        // NOTE: I have spent enough time on this (damn) project and have no need to make it infinitely flexible.
        // Used by GetJsonGroups():
        const int DESIGN_LIMIT_HIERARCHY_MAXIMUM = 3;

        // Used by: AssembleQueryPathString(), ConstructJsonStringFromList() and GetJsonGroups():
        int databaseSectionCount;

        // Used by PathExtrapolationPrep() and PathExtrapolation():
        int queryResponseSectionCount;

        // Used by InputCriteria() and PathExtrapolationPrep:
        List<string> idList = new List<string>();
        List<KeyValuePair<string, string>> criteriaKeyValuePair = new List<KeyValuePair<string, string>>();

        // Constructor:
        public JsonPath()
        {
            //// Turn our JSON string into a JSONPath JSON object:
            //jObjectFullDatabase = JObject.Parse(@fullDatabase);

            //// Peruse the database schema and find all the array names and the first items (only) within them.
            //// This information drives much of the following processing:
            //GetJsonGroups();

        }

        // The remaining methods are in alphabetical order:
        public List<List<String>> AssembleQueryPathString()
        {
            // Init:
            List<List<String>> finalJsonContentList  = new List<List<String>>();
            List<string>       levelJsonStrings      = new List<string>(new string[databaseSectionCount]);

            for (int outerIndex = 0; outerIndex < pathLevelIndicesParent.Count; outerIndex++)
            {
                // Init a new list for the outerIndex level of our two-dimensional list:
                finalJsonContentList.Add(new List<String>());

                string pathString = ""; // init

                // Go through all the path levels individually:
                for (int innerIndex = 0; innerIndex < pathLevelIndicesParent[outerIndex].Length; innerIndex++)
                {
                    // Assemble the path string that we will query on:
                    if (innerIndex > 0) { pathString += "."; }

                    pathString += arrayKeyValuePairAllSections[innerIndex].Key + "[" + pathLevelIndicesParent[outerIndex].GetValue(innerIndex).ToString() + "]";    // eg loop 0: "Discography[0]                      "
                                                                                                                                                                    // eg loop 1: "Discography[0].sections[0]          "
                    IEnumerable<JToken> jTokens = jObjectFullDatabase.SelectTokens(pathString);                                                                     // eg loop 2: "Discography[0].sections[0].albums[0]"
                    // Query on the path string.                                                                                                                     
                    // NOTE: jTokenItem values are all we need to piece together a narrow query-response discography:
                    foreach (JToken jTokenItem in jTokens)  // [0] the full Til Tuesday discography:              {{"id":"Til Tuesday",..."album":"Voices Carry"}]}]}}
                                                            // [1] the section portion of the searched-for album: {{"section": "Albums",..."album": "Everything's Different Now"}]}}
                    {                                       // [2] the album portion of the searched-for album:   {{"year": 1985, "album": "Voices Carry"}}
                        // Capture the above-described items in a string array:
                        levelJsonStrings[innerIndex] = jTokenItem.ToString();

                        // Perform "surgery" on all JSON array descriptions because we are going to reconstitute them (more narrowly).
                        // Basically, truncate them after the opening bracket:
                        int bracketPosition = levelJsonStrings[innerIndex].IndexOf("[");
                        if (bracketPosition > -1) { levelJsonStrings[innerIndex] = levelJsonStrings[innerIndex].Substring(0, bracketPosition + 1); }

                        // Load the final list, a new entry for the innerIndex level of our two-dimensional list:
                        finalJsonContentList[outerIndex].Add(levelJsonStrings[innerIndex]);

                        // We have all we need so terminate the loop:
                        break;
                    }
                }

            }

            // Return our two-dimensional list:
            return finalJsonContentList;

        }
        public string Cleanup(string jsonResponseString)
        {
            // Since we will be modifying the argument, let's make a copy and work with that:
            string jsonResponseStringEdited = jsonResponseString;

            // Clean up the crap that JSONPath attaches to its JSON:
            jsonResponseStringEdited = jsonResponseStringEdited.Replace("\r", "");
            jsonResponseStringEdited = jsonResponseStringEdited.Replace("\n", "");
            jsonResponseStringEdited = jsonResponseStringEdited.Replace(@"\", "");

            // Return the modified copy of the argument:
            return jsonResponseStringEdited;

        }
        public string ConstructJsonStringFromList(List<List<String>> finalJsonContentList)
        {
            int hierarchyLowest = databaseSectionCount - 1; // eg: 3 - 1 = 2

            // Handle the first entry:
            bool controlBreakInProg = true; // init

            string finalJsonString = ""; // init

            // Go through our two-dimensional JSON content list and construct the final JSON structure as a string:
            for (int outerIndex = 0; outerIndex < finalJsonContentList.Count; outerIndex++)                     // finalJsonContentList.Count = first dimension
            {
                  for (int innerIndex = 0; innerIndex < finalJsonContentList[outerIndex].Count; innerIndex++)   // finalJsonContentList[outerIndex].Count = second dimension
                {
                    // Handle "control breaks" on subsequent entries:
                    if (controlBreakInProg == false)
                    {
                        if (finalJsonContentList[outerIndex][innerIndex] != finalJsonContentList[outerIndex - 1][innerIndex]) { controlBreakInProg = true; }
                    }
                    // The lowest (in the hierarchy) is the detail.  It prints all the time.  Otherwise, just control breaks print:
                    if (innerIndex == hierarchyLowest || controlBreakInProg == true) { finalJsonString += finalJsonContentList[outerIndex][innerIndex]; }

                }

                controlBreakInProg = false; // reset

            }

            return finalJsonString;

        }
        public string EndOfSection(string jsonResponseString)
        {
            // Since we will be modifying the argument, let's make a copy and work with that:
            string jsonResponseStringEdited = jsonResponseString;

            // Since we did not account for these when loading we will do so now.

            // Beginning of response at the beginning of the string:
            jsonResponseStringEdited =
                "{ \"" + arrayKeyValuePairAllSections[0].Key + "\":[" + jsonResponseStringEdited;                       // eg: Discography  // LEVEL HARDCODE

            const string SECTION_TERMINATOR = "]}";

            // End of section, intermediate, scattered throughout the string:
            jsonResponseStringEdited =
                jsonResponseStringEdited.Replace("}{  \"" + arrayKeyValuePairAllSections[0].Value, "}" +                                    // LEVEL HARDCODE
                    string.Concat(Enumerable.Repeat(SECTION_TERMINATOR, 2)) + ", {  \"" +                                                   // LEVEL HARDCODE
                    arrayKeyValuePairAllSections[0].Value);                                                             // eg: id               // LEVEL HARDCODE

            //- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
            // DOWNSIZE KLUDGE
            try
            {
                jsonResponseStringEdited =
                    jsonResponseStringEdited.Replace("}{  \"" + arrayKeyValuePairAllSections[1].Value, "}" +                                // LEVEL HARDCODE
                        string.Concat(Enumerable.Repeat(SECTION_TERMINATOR, 1)) + ", {  \"" +                                               // LEVEL HARDCODE
                        arrayKeyValuePairAllSections[1].Value);                                                         // eg: section      // LEVEL HARDCODE

                jsonResponseStringEdited =
                    jsonResponseStringEdited.Replace("}{  \"" + arrayKeyValuePairAllSections[2].Value, "}, {  \"" +                         // LEVEL HARDCODE
                    arrayKeyValuePairAllSections[2].Value);                                                             // eg: album        // LEVEL HARDCODE

            }
            catch (Exception) { }
            //- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 

            // End of section, final, at the end of the string:
            jsonResponseStringEdited += string.Concat(Enumerable.Repeat(SECTION_TERMINATOR, databaseSectionCount));

            // Return the modified copy of the argument:
            return jsonResponseStringEdited;

        }
        public IEnumerable<JToken> ExecuteJsonPathQuery(List<string> idList, List<KeyValuePair<string, string>> criteriaKeyValuePair)
        {
            // Init:
            string query = "";
            string selectTokensQuery = "";

            // NOTE: The following two argument options are mutually-exclusive:

            if (idList.Count > 0)
            {
                // KEY LOOKUP:
                // @.id == 'Elton John' || @.id == 'The Police'

                foreach (string id in idList)
                {
                    if (query != "") { query += " || "; }
                    query += "@.id == '" + id + "'";
                }

                // Switch the naming to camel case:
                string entityCamelCase = ToCamelCaseFromPascalCase(entity);

                // Do final assembly of the JSONPath query:
                selectTokensQuery = "$.." + entityCamelCase + "[?(" + query + ")].id";

            }
            else
            {
                // CRITERIA LOOKUP:
                // @.decade == 1970 && @.genre == 'POP'

                // Look at the first search field:
                int searchIndex = fullDatabase.IndexOf(criteriaKeyValuePair[0].Key);
                searchIndex = fullDatabase.LastIndexOf("[", searchIndex - 1);

                // Search backwards to find the group that it is a member of:
                int quote2Pos = fullDatabase.LastIndexOf("\"", searchIndex - 1);
                int quote1Pos = fullDatabase.LastIndexOf("\"", quote2Pos - 1);

                // Extract the name of the group:
                string group = fullDatabase.Substring(quote1Pos + 1, quote2Pos - quote1Pos - 1); // eg: "artist"

                foreach (KeyValuePair<string, string> keyValuePair in criteriaKeyValuePair)
                {
                    if (query != "") { query += " && "; }

                    int value; // I don't need this but the function requires it

                    // If the value passed is a number...

                    //...don't wrap it in quotes...
                    if (int.TryParse(keyValuePair.Value, out value) == true) { query += "@." + keyValuePair.Key + " == " + keyValuePair.Value; }

                    //...otherwise, wrap it:
                    else { query += "@." + keyValuePair.Key + " == '" + keyValuePair.Value + "'"; }

                }

                // Do final assembly of the JSONPath query:
                selectTokensQuery = "$.." + group + "[?(" + query + ")]." + criteriaKeyValuePair[0].Key;

            }

            // Execute the JSONPath query:
            IEnumerable<JToken> jTokens = jObjectFullDatabase.SelectTokens(selectTokensQuery);

            // Return the result in the form of a JSONPath enumerable JSON token:
            return jTokens;

        }
        public List<int[]> GetExtrapolatedPathIndices(int outerIndex, int middleIndex)
        {
            // Unfortunately, JSONPath only returns values as deep in the hierarchy as they are found.  I want to drill down
            // to the bottom of the hierarchy so I must do so manually. On each level, I will "probe" to find the end of the
            // line.  All the "hits" will be saved and JSONPath's results will be "fleshed out" with my findings:

            List<int> outerMiddleInnerList = new List<int>();
            List<int[]> pathLevelIndicesFinalList = new List<int[]>();

            // Build the path we will search with as we go:
            string outerTestPath =
                arrayKeyValuePairAllSections[0].Key + "[" + outerIndex.ToString() + "]";                         // eg: "Discography[0]"             // LEVEL HARDCODE                       

            // Continue building the path:
            string middleTestPath =
                outerTestPath + "." + arrayKeyValuePairAllSections[1].Key + "[" + middleIndex.ToString() + "]"; // eg: "Discography[0].sections[0]" // LEVEL HARDCODE

            // Init:
            int innerIndex = 0;
            bool boolPathFound = false;

            do
            {
                //- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
                // DOWNSIZE KLUDGE
                // Continue building the path:
                string innerTestPath = "";
                try {
                    innerTestPath =
                        middleTestPath + "." + arrayKeyValuePairAllSections[2].Key +                            // eg: "Discography[0].sections[0].albums[0]" // LEVEL HARDCODE
                        "[" + innerIndex.ToString() + "]";
                }
                catch (Exception)
                {
                    innerTestPath = middleTestPath;
                }
                //- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 

                // Lookup the prospective set of indices in the database:
                boolPathFound = PathFound(innerTestPath);

                // If they are found:
                if (boolPathFound == true)
                {
                    // Add indices to list:
                    outerMiddleInnerList.Add(outerIndex);
                    outerMiddleInnerList.Add(middleIndex);
                    //- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
                    // DOWNSIZE KLUDGE
                    if (databaseSectionCount == 3) { outerMiddleInnerList.Add(innerIndex); }    // eg: [{0, 0, 0}]  // LEVEL HARDCODE
                    //- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

                    // Add list to array:
                    int[] outerMiddleInnerArray = outerMiddleInnerList.ToArray();               // eg: [{0, 0, 0}]

                    // Add array to list:                                                       // At end:
                    pathLevelIndicesFinalList.Add(outerMiddleInnerArray);                       // [0] {0, 0, 0}                   
                    outerMiddleInnerList.Clear();                                               //  .
                                                                                                //  .
                }                                                                               // [7] {1, 1, 1}
                // For the next time through the loop:
                innerIndex++;
                //- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
                // DOWNSIZE KLUDGE
                if (databaseSectionCount == 2) { if (innerIndex == 1) { boolPathFound = false; } }                  // LEVEL HARDCODE
                //- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

                // We will stop when we reach the end of the line:
            } while (boolPathFound == true);

            // Return the list of extrapolated indices:
            return pathLevelIndicesFinalList;

        }
        public List<int[]> GetExtrapolatedPathIndicesMain(int parentIndex, int childIndex)
        {
            // Init:
            List<int[]> pathLevelIndicesFinalList     = new List<int[]>();
            List<int[]> pathLevelIndicesFinalListLook = new List<int[]>();

            // If the query was of a detail-level field, there is no need to extrapolate.  JSONPath has already
            // supplied us:
            if (childIndex > -1) { pathLevelIndicesFinalList = GetExtrapolatedPathIndices(parentIndex, childIndex); }
            else
            {
                // If the query was of a group-level field, we must extrapolate:
                do
                {
                    // Do a lookup:
                    childIndex++;
                    pathLevelIndicesFinalListLook = GetExtrapolatedPathIndices(parentIndex, childIndex);

                    // If a value was found, save it:
                    if (pathLevelIndicesFinalListLook.Count > 0) { pathLevelIndicesFinalList.AddRange(pathLevelIndicesFinalListLook); }

                // Continue until we reach the end of the line:
                } while (pathLevelIndicesFinalListLook.Count > 0);

            }

            // Return the result:
            return pathLevelIndicesFinalList;

        }
        public void GetJsonGroups()
        {
            // NOTE: I tried getting this information from JSONPath, but it is easier
            // to just search through the JSON string:

            // Get one "record":
            string workSchema = fullDatabase.Substring(0, fullDatabase.IndexOf("]") + 1); // eg: "{\"Discography\":[{\"id\":\"Til Tuesday\",\"wikipedia_page\":\"http://en.wikipedia.org/wiki/'Til_Tuesday\",\"sections\":[{\"section\":\"Albums\",\"style\":1,\"albums\":[{\"year\":1985,\"album\":\"Voices Carry\"},{\"year\":1986,\"album\":\"Voices Carry\"},{\"year\":1988,\"album\":\"Everything's Different Now\"}]"

            int searchStartPos = 0; // init

            do
            {
                // Get the first array definition and break it down to the components that I care about:
                int arrayBeginPos = workSchema.IndexOf("[", searchStartPos);

                    if (arrayBeginPos > -1)
                    {
                        // NOTE: The array name and first item "straddle" the bracket.  We will search backward and forward from it:
                        int quote2Pos = workSchema.LastIndexOf("\"", arrayBeginPos); // eg: 13
                        int quote1Pos = workSchema.LastIndexOf("\"", quote2Pos - 1); // eg: 1
                        int quote3Pos = workSchema.IndexOf(    "\"", arrayBeginPos); // eg: 17
                        int quote4Pos = workSchema.IndexOf(    "\"", quote3Pos + 1); // eg: 20

                        // Save these in a list:
                        string groupName     = workSchema.Substring(quote1Pos + 1, quote2Pos - quote1Pos - 1);          // eg: "Discography"
                        string firstItemName = workSchema.Substring(quote3Pos + 1, quote4Pos - quote3Pos - 1);          // eg: "id"
                        arrayKeyValuePairAllSections.Add(new KeyValuePair<string, string>(groupName, firstItemName));   // eg: {["Discography", "id"]}

                        // Prep for the next time through the loop
                        searchStartPos = arrayBeginPos + 1;

                    }

                // The search is over:
                else { searchStartPos = - 1; }

            } while (searchStartPos > -1);

            // arrayKeyValuePairAllSections content eg:
            // {["Discography", "id"     ]}
            // {["sections"   , "section"]}
            // {["albums"     , "year"   ]}

            // Save this because it's very important and will be needed everywhere:
            databaseSectionCount = arrayKeyValuePairAllSections.Count;

            // NOTE: I have spent enough time on this (damn) project and
            // have no need to make it infinitely flexible:
            if (databaseSectionCount > DESIGN_LIMIT_HIERARCHY_MAXIMUM)
            {
                // Construct the error message:
                string errorMessage =
                    "Database section count (" + databaseSectionCount.ToString() +
                    ") exceeds design limit hierarchy maximum (" +
                    DESIGN_LIMIT_HIERARCHY_MAXIMUM.ToString() + ").";

                // Write the error messages and halt:
                Debug.WriteLine(errorMessage);
                Console.WriteLine(errorMessage);
                Environment.Exit(databaseSectionCount);

            }

        }
        public string InputCriteria(HttpRequestWrapper Request)
        {
            //-----------------------------------------------------------------------------------------------------------------
            // ARGUMENT OPTION 1 OF 5 (development/debug only) - This only occurs in development debug when the
            // following, absurd path occurs: "/Artist/Get/Index.aspx".  NOTE: This is probably because I created
            // this form subsequent to initial application creation and it is somehow running afoul of MVC.

            // NOTE: This same issue causes me to have to code shortcuts explicitly to be sure they will work
            // properly, eg: "www.workingweb.info/database/Index.aspx" instead of "www.workingweb.info/database/".

            // Eg: "/Artist/Get/Index.aspx"

            if (Request.Path.IndexOf("Index.aspx") > -1) { return "Development error.  Re-request desired page."; }

            //-----------------------------------------------------------------------------------------------------------------
            // Get the ASP MVC database "entity":

            // Request.Path eg. values:
            // Development: /Artist/Get/
            // Production:  /database/Artist/Get/
            //              01234567890

            // For testing:
          //string requestPath = "/Artist/Get/";            // development
          //string requestPath = "/database/Artist/Get/";   // production
            string requestPath = Request.Path;

            int searchStartPos;
            if (requestPath.IndexOf("/database/", StringComparison.OrdinalIgnoreCase) > -1) { searchStartPos = 10; } // production
            else { searchStartPos = 1; }                                                                             // development

            // Save it for later use:
            this.entity = requestPath.Substring(searchStartPos, requestPath.IndexOf("/", searchStartPos) - searchStartPos);

            // The "database" is stored as JSON in a file named for the Entity (eg: "Artist"):
            this.fullDatabase = System.IO.File.ReadAllText(HostingEnvironment.MapPath(@"~/Models/" + this.entity + ".json"));

            // Turn our JSON string into a JSONPath JSON object:
            jObjectFullDatabase = JObject.Parse(@fullDatabase);

            // Peruse the database schema and find all the array names and the first items (only) within them.
            // This information drives much of the following processing:
            GetJsonGroups();
            //-----------------------------------------------------------------------------------------------------------------
            // ARGUMENT OPTION 2 OF 5 (production/non-debug) - No arguments are supplied, implying that all items are desired:

            // Egs: "/Artist/Get/" or "/Artist/Get" or "/Artist/get/" or "/Artist/get"

            // Examine the end of the path:
            string getSlash = Request.RawUrl.Substring(Request.RawUrl.Length - 4); // eg: "Get/"
            string get      = Request.RawUrl.Substring(Request.RawUrl.Length - 3); // eg: "Get"

            // "String.Compare()" compares two strings and returns an integer indicating their relative position in the
            // sort order.  Zero means there is a match.  We are accounting for case and the presence or absence of a
            // trailing slash:
            int getSlashCompare = String.Compare(getSlash, "Get/", true); // ignore case = TRUE
            int getCompare      = String.Compare(get,      "Get",  true); // ignore case = TRUE

            // If the path ends in "Get/" or "Get"...
            if (getSlashCompare == 0 || getCompare == 0)
            {
                //...it means that the calling module has not supplied any arguments.  This is like a SQL
                // "Select all".  Since the "database" is just a JSON string, return it in its entirety:
                return this.fullDatabase;

            }
            //-----------------------------------------------------------------------------------------------------------------
            // ARGUMENT OPTION 3 OF 5 (production/non-debug) - One argument is supplied but with no field name, implying that
            // the passed value is an ID field as is standard behavior in MVC.

            // Eg: "/Artist/Get/Artist+0"

            // If we have an ARGUMENT OPTION 3 OF 5 situation:
            if (Request.RawUrl.IndexOf("?") == -1)
            {
                // Isolate the ID value, clean-up the transport packaging and load it into the ID list:
                int lastSlashPos = Request.RawUrl.LastIndexOf("/", Request.RawUrl.Length);
                string id = Request.RawUrl.Substring(lastSlashPos + 1, Request.RawUrl.Length - lastSlashPos - 1);
                id = id.Replace("+", " ");
                idList.Add(id);

                // This will trigger JSONPath lookup processing:
                return "";

            }
            else
            {
                foreach (String key in Request.QueryString.AllKeys)
                {
                    // Clean-up the transport packaging:
                    string requestQueryString = Request.QueryString[key].Replace("+", " ");

                    // NOTE: The following two argument options are mutually-exclusive:

                    //-------------------------------------------------------------------------------------------------------
                    // ARGUMENT OPTION 4 OF 5 (production/non-debug) - One or more ID arguments are supplied with matching
                    // field names:

                    // Eg: "/Artist/Get/?id=Artist+0"

                    // Load the ID list or...
                    if (key.IndexOf("id") > -1) { idList.Add(requestQueryString); }

                    //-------------------------------------------------------------------------------------------------------
                    // ARGUMENT OPTION 5 OF 5 (production/non-debug) - One or more non-ID arguments are supplied with
                    // matching field names:

                    // Eg: "/Artist/Get/?decade=1960&genre=ROCK"

                    //...load the Criteria list:
                    else { criteriaKeyValuePair.Add(new KeyValuePair<string, string>(key, requestQueryString)); }

                    //-------------------------------------------------------------------------------------------------------

                }

                // This will trigger JSONPath lookup processing:
                return "";

            }

        }
        public List<int[]> PathExtrapolation()
        {
            // Init:
            int parent_index = 0; // the highest level
            int child_index = parent_index + 1;
            List<int[]> extrapolatedIndicesListCombined = new List<int[]>();

            // NOTE: We have the parent and child indices.  Extrapolate to the inner/detail level's
            // indices, thus getting all children items.

            // If the query and its result are on a higher level than the lowest detail, then we have to drill down
            // (extrapolate) lower:
            if (queryResponseSectionCount < databaseSectionCount)
            {
                // Go through all of the response items returned by JSONPath:
                for (int resultItemIndex = 0; resultItemIndex < pathLevelIndicesParent.Count; resultItemIndex++)
                {
                    int parent_index_content = -1; // init required by the compiler
                    int child_index_content;

                    for (int resultArrayIndex = 0; resultArrayIndex < pathLevelIndicesParent[resultItemIndex].Length; resultArrayIndex++)
                    {
                        // Assign the parent and child properly:
                        if (resultArrayIndex == parent_index) { parent_index_content = (int)pathLevelIndicesParent[resultItemIndex].GetValue(resultArrayIndex); } 
                        else if (resultArrayIndex == child_index)
                        {
                            child_index_content = (int)pathLevelIndicesParent[resultItemIndex].GetValue(resultArrayIndex);

                            // We now have the parent and child indices.  Extrapolate to the inner/detail level's indices, thus getting
                            // all children items.  Merge it into the final result:
                            extrapolatedIndicesListCombined.AddRange(GetExtrapolatedPathIndicesMain(parent_index_content, child_index_content));
                        }
                    }

                    // Getting only 1 index indicates that we are at the top of the hierarchy:
                    if (pathLevelIndicesParent[resultItemIndex].Length == 1)
                    {
                        // -1 indicates that the query was of a group-level field.  The top of the hierarchy
                        // is always a group-level field:
                        extrapolatedIndicesListCombined.AddRange(GetExtrapolatedPathIndicesMain(parent_index_content, -1));
                    }

                }

            }

            // Return the fully extrapolated indices:
            return extrapolatedIndicesListCombined;

            //------------------------------------------------------------------------------------------------------
            // Example query: "$..sections[?(@.section == 'Section 0, 0' || @.section == 'Section 0, 1')].section"
            // Result:
            // [0] {0, 0, 0}
            // [1] {0, 0, 1}
            // [2] {0, 1, 0}
            // [3] {0, 1, 1}

        }
        public void PathExtrapolationPrep(HttpRequestWrapper Request)
        {
            //---------------------------------------------------------------------------------------------------------
            // SelectToken() provides a method to query LINQ to JSON using a single string path to a desired JToken.
            // SelectToken makes dynamic queries easy because the entire query is defined in a string.

            // NOTE: The appeal of JSONPath here is that it will do the JSONPath (an implementation of X-Path)
            // querying for me.  However I'm going against the grain because the utility returns tokened "items",
            // one for every database column where I prefer to simply work with database rows.  To accomplish this,
            // I observed the process in operation via the debugger and found where the row-based structure existed
            // and then grabbed it at that point.

            //---------------------------------------------------------------------------------------------------------
            // Execute the JSONPath query getting the result in the form of a JSONPath enumerable JSON token:
            IEnumerable<JToken> jTokens = this.ExecuteJsonPathQuery(idList, criteriaKeyValuePair); // 2nd arg. eg 1: {[albums.album    , Voices Carry]}
                                                                                                   //          eg 2: {[sections.section, Albums      ]}
            // Init:
            List<int> pathLevelIndicesChild = new List<int>();
            List<string> pathLevelSections = new List<string>();

            // Despite the name, this is a conventional C# list:
            List<string> jsonList = new List<string> { };

            foreach (JToken jTokenItem in jTokens)
            {
                // JSONPath wants to work with columns but I want to work with rows
                // (ie, the processing that follows):
                JValue jValue = (JValue)jTokenItem; // eg: {Voices Carry}

                List<string> pathLevelAll = new List<string>();

                // NOTE: This is the "key" to our result.  Now we must get its supporting data.
                // eg: "Discography[0].sections[0].albums[0].album"
                pathLevelAll.AddRange(jValue.Path.Split('.'));  // eg 1: { "Discography[0]", "sections[0]", "albums[0]", "album" }
                                                                // eg 2: { "Discography[0]", "sections[0]", "section"            }
                // We only need to do this once:
                if (pathLevelSections.Count == 0)
                {
                    for (int i = 0; i < pathLevelAll.Count - 1; i++)
                    {
                        pathLevelSections.Add(pathLevelAll[i].Substring(0, pathLevelAll[i].IndexOf("["))); // eg 1: { "Discography", "sections", "albums" }
                    }                                                                                      // eg 2: { "Discography", "sections"           }
                }

                // Save this:
                queryResponseSectionCount = pathLevelSections.Count;

                for (int i = 0; i < queryResponseSectionCount; i++)
                {
                    // Get and save the dimension size between the brackets:
                    int start = pathLevelAll[i].IndexOf("[") + 1;                   // eg: Discography[0]
                    pathLevelIndicesChild.Add(Int32.Parse(pathLevelAll[i].Substring(start, pathLevelAll[i].IndexOf("]") - start)));     // eg 1: {0, 0, 0}
                }                                                                                                                       // eg 2: {0, 0   }

                // Load the inner dimension:
                int[] pathLevelIndicesChildArray = pathLevelIndicesChild.ToArray(); // eg 1: [{0, 0, 0}]
                                                                                    // eg 2: [{0, 0   }]
                // Load the outer dimension:
                pathLevelIndicesParent.Add(pathLevelIndicesChildArray);             // eg 1: [{0, 0, 0}, {0, 0, 1}, {0, 1, 0}, {1, 0, 0}]
                pathLevelIndicesChild.Clear();                                      // eg 2: [{0, 0   }, {1, 0   }

            }

        }
        public bool PathFound(string pathString)
        {
            // Query on the path string for testing purposes only:
            IEnumerable<JToken> jTokens = jObjectFullDatabase.SelectTokens(pathString);

            bool found = false; // init
            foreach (JToken jTokenItem in jTokens)
            {
                found = true;
                break;
            }

            // Return the result:
            return found;

        }
        public string QueryJsonPathMain(HttpRequestWrapper Request)
        {
            // NOTE: This function acts as a job stream:

            // Handle input criteria:  Get the ASP MVC database "entity", load the criteria ID list and the other criteria list.
            string jsonResponseString = InputCriteria(Request);

            // If the query criteria did not "Select All":
            if (jsonResponseString == "")
            {
                // NOTE: This method fills "pathLevelIndicesParent".
                // Use JSONPath's SelectToken() to extrapolate to the inner/detail level's indices:
                PathExtrapolationPrep(Request);

                // If we did not get a detail level response...
                if (queryResponseSectionCount < databaseSectionCount)
                {
                    // ...complete the extrapolation to the inner level's indices, thus getting all children items.  Overlay the
                    // unextrapolated index list with the fully extrapolated index list:
                   pathLevelIndicesParent = PathExtrapolation();
                }

                // Go through all the path levels individually and assemble the path string that we will query on:
                List<List<String>> finalJsonContentList = AssembleQueryPathString();

                // Go through our two-dimensional JSON content list and construct the final JSON structure as a string:
                jsonResponseString = ConstructJsonStringFromList(finalJsonContentList);

                // Clean up the crap that JSONPath attaches to its JSON:
                jsonResponseString = Cleanup(jsonResponseString);

                // Since we did not account for end-of-section when loading we will do so now.
                jsonResponseString = EndOfSection(jsonResponseString);

            }

            // Return the result:
            return jsonResponseString;

        }
        public string ToCamelCaseFromPascalCase(string variable)
        {
            // NOTE: 1) PascalCase, 2) camelCase

            // Convert the variable name (eg: "Artist") to an array of characters:
            char[] variableCharArray = variable.ToCharArray();

            // Change the first letter of the variable name to lower case, thus
            // affecting "camel case" (eg: "artist"):
            variableCharArray[0] = Char.ToLower(variableCharArray[0]);
            string variableCamelCase = new string(variableCharArray);

            // Return the result:
            return variableCamelCase;

        }

    }

}
