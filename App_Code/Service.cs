using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Xml;
using System.Net;
using System.IO;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service" in code, svc and config file together.
public class Service : IService
{
    /*
     * Return: string array
     * Argument: string
     * Purpose: given a zipcode, returns a string array with either the 4 or 5 day forecast
     */
	public string[] Weather5day(string zipcode)
    {
        //This variable is for error checking
        string[] returner = new string[] { "Incorrect ZipCode" };
		WeatherService.ndfdXML client = new WeatherService.ndfdXML();

        DateTime aDate = DateTime.Now;
        string xml = "";
        int numericValue;
        //Checks if the zipcode is actually a string
        bool isNumber = int.TryParse(zipcode, out numericValue);
        if(!isNumber)
        {
            return returner;
        }
        //Makes sure the zipcode has length 5, if it has length less then the API returns an error
        if(zipcode.Length < 5)
        {
            return returner;
        } 
        //Pulls a string containting the latitude and longitude in xml format from the API client
        xml = client.LatLonListZipCode(zipcode);

       
        //Make a text reader and variables
		XmlTextReader reader2 = new XmlTextReader(new System.IO.StringReader(xml));
		string test1 = "";
		string test2 = "";

		reader2.WhitespaceHandling = WhitespaceHandling.None;
        //Loop to go through the xml document
		while (reader2.Read())
		{
			//The way the XML is formatted, the Text nodes have the longitude and latitude in them
			if (reader2.NodeType.ToString().Equals("Text"))
			{
                //Store the longitude and latitude
				string latlon = reader2.Value.ToString();
				String[] latlonarray = latlon.Split(',');
				test1 = latlonarray[0];
				test2 = latlonarray[1];

			}
		}
        //If there wasn't a longitude and latitude then that means an incorrect zipcode was used
        if(test1.Equals(""))
        {
            return returner;
        }
		decimal lattitude = System.Convert.ToDecimal(test1);
		decimal longitude = System.Convert.ToDecimal(test2);
        //Gets an xml string from the client which features the 5 day forecast
		xml = client.NDFDgenByDay(lattitude, longitude, aDate, "5", "e", "12 hourly");

        
        //Creates another reader to process the new xml string
		XmlTextReader reader = new XmlTextReader(new System.IO.StringReader(xml));
		reader.WhitespaceHandling = WhitespaceHandling.None;
		int i = 0;
		Boolean tester = true;
		Boolean tester2 = true;
        //Creates 3 list for holding the max temps, min temps, and cloud/sun forecast
		List<string> maxTemps = new List<string>();
		List<string> minTemps = new List<string>();
		List<string> weather = new List<string>();
        while (reader.Read())
        {
            //Goes into the Daily Max Temp variables
            if (reader.Value.ToString().Equals("Daily Maximum Temperature"))
            {
                //Need to iterate once so it doesn't count itself
                reader.Read();
                while (tester)
                {
                    //Nodes with the type "Text" carry the MaxTemps values
                    if (reader.NodeType.ToString().Equals("Text"))
                    {
                        //Add the temps in a list
                        maxTemps.Add(reader.Value.ToString());

                    }
                    //This is when we know we are at the end of the Max Temps values
                    else if (reader.Name.ToString().Equals("temperature"))
                    {
                        //Leave the loop
                        tester = false;
                    }
                    reader.Read();
                }
            }
            //Goes into the Daily Min Temps values
            if (reader.Value.ToString().Equals("Daily Minimum Temperature"))
            {
                //Iterate so it doesn't count itself
                reader.Read();
                while (tester2)
                {
                    //Nodes with the type text carry the values
                    if (reader.NodeType.ToString().Equals("Text"))
                    {
                        //Add up the values
                        minTemps.Add(reader.Value.ToString());
                    }
                    //This is when we are at the end of the element
                    else if (reader.Name.ToString().Equals("temperature"))
                    {
                        tester2 = false;
                    }
                    reader.Read();
                }
            }
            //Goes into the attributes since the forecast is an attribute in this XML file
            if (reader.AttributeCount > 0)
            {
                while (reader.MoveToNextAttribute())
                {
                    //Checks for the weather summary attribute and adds it up
                    if (reader.Name.ToString().Equals("weather-summary"))
                    {
                        weather.Add(reader.Value.ToString());
                    }

                }
            }
        }
        //This is the return value
        string[] test;
        int j = 0;
        /*
         * The XMl returned from the API sometimes returns only 4 max temps, 5 min temps, and 9 weather summary
         * This is because when it is called, it is too late in the day so it no longer has the max temp value
         */
        if (maxTemps.Count == 4 && minTemps.Count == 5)
        {
            test = new string[5];
            test[0] = "Daily Max: Too late in the day" + " Daily Min: " + minTemps[0] + " Sun: Too late in the day" + " Clouds: " + weather[0];
            test[1] = "Daily Max: " + maxTemps[0] + " Daily Min: " + minTemps[1] + " Sun: " + weather[1] + " Clouds: " + weather[2];
            test[2] = "Daily Max: " + maxTemps[1] + " Daily Min: " + minTemps[2] + " Sun: " + weather[3] + " Clouds: " + weather[4];
            test[3] = "Daily Max: " + maxTemps[2] + " Daily Min: " + minTemps[3] + " Sun: " + weather[5] + " Clouds: " + weather[6];
            test[4] = "Daily Max: " + maxTemps[3] + " Daily Min: " + minTemps[4] + " Sun: " + weather[7] + " Clouds: " + weather[8];


        }
        //This is if the XMl returns a full 5 day forecast
        else if (maxTemps.Count == 5 && minTemps.Count == 5)
        {
            test = new string[5];
            test[0] = "Daily Max: " + maxTemps[0] + " Daily Min: " + minTemps[0] + " Sun: " + weather[0] + " Clouds: " + weather[1];
            test[1] = "Daily Max: " + maxTemps[1] + " Daily Min: " + minTemps[1] + " Sun: " + weather[2] + " Clouds: " + weather[3];
            test[2] = "Daily Max: " + maxTemps[2] + " Daily Min: " + minTemps[2] + " Sun: " + weather[4] + " Clouds: " + weather[5];
            test[3] = "Daily Max: " + maxTemps[3] + " Daily Min: " + minTemps[3] + " Sun: " + weather[6] + " Clouds: " + weather[7];
            test[4] = "Daily Max: " + maxTemps[4] + " Daily Min: " + minTemps[4] + " Sun: " + weather[8] + " Clouds: " + weather[9];

        }
        //Sometimes, if its very late in the day, the API XML returns only a 4 day forecast 
        else if (maxTemps.Count == 4 && minTemps.Count == 4)
        {
            test = new string[4];
            test[0] = "Daily Max: " + maxTemps[0] + " Daily Min: " + minTemps[0] + " Sun: " + weather[0] + " Clouds: " + weather[1];
            test[1] = "Daily Max: " + maxTemps[1] + " Daily Min: " + minTemps[1] + " Sun: " + weather[2] + " Clouds: " + weather[3];
            test[2] = "Daily Max: " + maxTemps[2] + " Daily Min: " + minTemps[2] + " Sun: " + weather[4] + " Clouds: " + weather[5];
            test[3] = "Daily Max: " + maxTemps[3] + " Daily Min: " + minTemps[3] + " Sun: " + weather[6] + " Clouds: " + weather[7];

        }
        //Used if there was nothing stored in the strings
        else
        {
            test = new string[1];
            test[0] = "There is nothing in here";
        }
        return test; 
		

    
	
    }
    /*
     * Return Type: decimal
     * Parameters: decimal, decimal
     * Purpose: Uses an API to return the average Normal Irradiance value for a particular lat and lon
     */
    public String SolarIntensity(String latitude, String longitude)
    {

        decimal dLat = Convert.ToDecimal(latitude);
        decimal dLon = Convert.ToDecimal(longitude);
        //The API can only process lats within these values
        if (dLat > 90 || dLat < -90)
        {
            return "0";
        }
        //The API can only process lons within these values
        else if (dLon > 180 || dLon < -180)
        {
            return "0";
        }
        //This is the RESTful service call
        string apiKey = "";
        string url = "https://developer.nrel.gov/api/solar/solar_resource/v1.xml?api_key=" + apiKey + latitude + "&lon=" + longitude;
        //Use a WebRequest to create, get the response, and stream it into a string
        //This code was taken from teammate EZ, who received it from the internet
        WebRequest request = WebRequest.Create(url);

        WebResponse response = request.GetResponse();

        Stream data = response.GetResponseStream();

        StreamReader Sreader = new StreamReader(data);

        string responseFromServer = Sreader.ReadToEnd();

        //Create an XML reader since the response was in XML
        XmlTextReader reader = new XmlTextReader(new System.IO.StringReader(responseFromServer));

        reader.WhitespaceHandling = WhitespaceHandling.None;

        List<string> solarAverage = new List<string>();
        Boolean tester = true;
        int j = 0;
        while (reader.Read())
        {
            //This is how we know we are about to get the sunny index average
            if (reader.Value.ToString().Equals("Perez-SUNY/NREL, 2012"))
            {
                reader.Read();
                while (tester)
                {
                    //There are 3 nodes with Text value, the lat, the lon, and the average for the year
                    if (reader.NodeType.ToString().Equals("Text"))
                    {
                        //We add them up in the SolarAverage
                        solarAverage.Add(reader.Value.ToString());
                        j++;
                    }
                    //Once we hit 3 we can leave because we do not need the rest
                    if (j == 3)
                    {
                        tester = false;
                    }
                    reader.Read();
                }
            }
        }
        //If there is no data, the third spot will say so in the XML so this is needed in those cases
        if (solarAverage[2].Equals("no data"))
        {
            return "0";
        }
        //However, if there is data, it will be converted to decimal and sent back
        else
        {
            decimal average;
            average = System.Convert.ToDecimal(solarAverage[2]);
            String averageString = average.ToString();
            return averageString;
        }
    }
}
