using System;
using System.Collections.Generic;
using System.Text;

namespace Exercise.Domain.Services
{
    public static class SampleData
    {

            public static string JsonFile = @"{
	'meeting': {    
		'id': '9001',
		'name': 'Canterburry',
		'state': 'NSW',
		'Date': '2017-09-01',
		'races': [{
			'id': '90011001',
			'racenumber': '1',
			'racename': 'Australian Turf Club',
			'starttime': '2017-09-01 10:45:00',
			'endtime': '2017-09-01 11:00:00'
		},
		{
			'id': '90011002',
			'racenumber': '2',
			'racename': 'Tab Rewards Maiden Hcp',
			'starttime': '2017-09-01 11:15:00',
			'endtime': '2017-09-01 11:35:00'
		},
		{
			'id': '90011003',
			'racenumber': '3',
			'racename': 'Tab Rewards Maiden Hcp',
			'starttime': '2017-09-01 15:05:00',
			'endtime': '2017-09-01 15:20:00'
		}]
	}
}";
        }
    }

