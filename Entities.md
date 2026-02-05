# Entities

Article(Id:Guid,Title:String,Summary:String,Content:String,Tags:String,Status:ArticleStatus,CreatedAt:DateTime,UpdatedAt:DateTime?)
QuizQuestion(Id:Guid,Category:String,QuestionText:String,IsActive:Bool,Options:ICollection<QuizOption>)
QuizOption(Id:Guid,QuestionId:Guid,OptionText:String,OptionValue:String,Question:QuizQuestion?)
McqResponse(Id:Guid,UserId:Guid,QuestionId:Guid,AnswerValue:String,AnsweredAt:DateTime,User:User?,Question:QuizQuestion?)
PoopType(Id:Guid,Name:String,Description:String)
EnergyLevel(Id:Guid,Name:String,Description:String)
GeminiQuestion(Id:Guid,QuestionText:String,Category:String,IsActive:Bool)

User(Id:Guid,Email:String,Phone:String?,AuthProvider:AuthProvider,AuthProviderUserId:String,IsEmailVerified:Bool,IsActive:Bool,CreatedAt:DateTime,LastSignInAt:DateTime?,Profile:UserProfile?,PrakritiQuizResponses:ICollection<PrakritiQuizResponse>,PrakritiResults:ICollection<PrakritiResult>,HealthSignals:ICollection<HealthSignal>,ChronicConditions:ICollection<ChronicCondition>,LifestyleProfiles:ICollection<UserLifestyleProfile>,VikritiSnapshots:ICollection<VikritiSnapshot>,CouponRedemptions:ICollection<CouponRedemption>,UserUsages:ICollection<UserUsage>,McqResponses:ICollection<McqResponse>)
UserProfile(UserId:Guid,FirstName:String,LastName:String,Gender:Gender,DateOfBirth:DateTime?,WeightLbs:Decimal?,HeightFeet:Int?,HeightInches:Int?,Country:String?,Timezone:String?,PreferredLanguage:String?,User:User?)

PrakritiQuizResponse(Id:Guid,UserId:Guid,QuestionId:Guid,AnswerValue:String,IsActive:Bool,User:User?,Question:QuizQuestion?)
PrakritiResult(Id:Guid,UserId:Guid,VataPercent:Decimal,PittaPercent:Decimal,KaphaPercent:Decimal,PrakritiLabel:DoshaType,IsActive:Bool,CalculatedAt:DateTime,User:User?)

Coupon(Id:Guid,Code:String,PlanType:PlanType,MaxRedemptions:Int,RedeemedCount:Int,ExpiryDate:DateTime?,IsActive:Bool,CreatedAt:DateTime,Redemptions:ICollection<CouponRedemption>)
CouponRedemption(Id:Guid,CouponId:Guid,UserId:Guid,RedeemedAt:DateTime,Coupon:Coupon?,User:User?)
UserUsage(Id:Guid,UserId:Guid,Date:DateOnly,ChatsUsed:Int,ArticlesUsed:Int,User:User?)
AccessPolicy(Id:Guid,PolicyType:PolicyType,MaxChatsPerDay:Int,MaxArticlesPerDay:Int,IsActive:Bool)

HealthSignal(Id:Guid,UserId:Guid,SignalType:SignalType,SignalValue:String?,NumericValue:Decimal?,ReportedAt:DateTime,Source:String,User:User?)
ChronicCondition(Id:Guid,UserId:Guid,ConditionType:String,Severity:String,DiagnosedAt:DateTime?,IsActive:Bool,User:User?)
UserLifestyleProfile(Id:Guid,UserId:Guid,NatureOfJob:String,TypicalWorkHours:String,PhysicalIntensity:String,User:User?)
VikritiSnapshot(Id:Guid,UserId:Guid,VataScore:Decimal,PittaScore:Decimal,KaphaScore:Decimal,DominantDosha:DoshaType,ReasonSummary:String,CalculatedAt:DateTime,User:User?)
HealthIndicator(Id:Guid,Name:String,Description:String,Category:String,IsActive:Bool)
