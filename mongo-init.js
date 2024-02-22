db.createUser(
    {
        user: "root",
        pwd: "root",
        roles: [
            {
                role: "readWrite",
                db: "attunedDB"
            }
        ]
    }
);

db = new Mongo().getDB("attunedDB");

db.createCollection('events', { capped: false });
db.createCollection('reservations', { capped: false });

db.events.insert(
    [{
        "name": "National Awareness Day",
        "description": "You won't have time for sleeping, soldier, not with all the bed making you'll be doing. Guess again. No, I'm Santa Claus! Bender! Ship! Stop bickering or I'm going to come back there and change your opinions manually!",
        "event_date": new Date("2027-03-05T13:00:00.000Z"),
        "availability": 10
    },
    {
        "name": "Universal Entrepreneurship Expo",
        "description": "I suppose I could part with 'one' and still be feared… Enough about your promiscuous mother, Hermes! We have bigger problems. Ummm…to eBay? Can I use the gun?",
        "event_date": new Date("2013-02-21T15:00:00.000Z"),
        "availability": 5
    },
    {
        "name": "Wine Festival",
        "description": "All I want is to be a monkey of moderate intelligence who wears a suit… that's why I'm transferring to business school! Meh. We'll go deliver this crate like professionals, and then we'll go home.",
        "event_date": new Date("2024-12-11T14:00:00.000Z"),
        "availability": 1
    },
    {
        "name": "Annual Bicycle Appreciation Day",
        "description": "Yes, if you make it look like an electrical fire. When you do things right, people won't be sure you've done anything at all. Oh dear! She's stuck in an infinite loop, and he's an idiot! Well, that's love for you",
        "event_date": new Date("2007-03-01T13:00:00.000Z"),
        "availability": 200
    },
    {
        "name": "Rocket to Mars",
        "description": "I'm nobody's taxi service; I'm not gonna be there to catch you every time you feel like jumping out of a spaceship. I'm the Doctor, I'm worse than everyone's aunt. *catches himself* And that is not how I'm introducing myself.",
        "event_date": new Date("2047-10-21T09:00:00.000Z"),
        "availability": 0
    }]
);